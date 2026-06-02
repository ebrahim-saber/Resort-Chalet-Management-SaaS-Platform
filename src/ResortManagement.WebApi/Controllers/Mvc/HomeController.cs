using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Identity;
using ResortManagement.Domain.Entities.Operations;
using MediatR;
using ResortManagement.Application.Features.Identity.Commands.LoginUser;
using ResortManagement.Application.Features.SaaS.Commands.RegisterTenant;
using Microsoft.AspNetCore.Http;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Route("")]
public class HomeController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMediator _mediator;

    private static readonly object _avatarLock = new();
    private static readonly object _gatewayLock = new();
    
    private static Dictionary<string, string> GetTenantGateways(string tenantId)
    {
        lock (_gatewayLock)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "billing", "gateways.json");
                if (!System.IO.File.Exists(path)) return new();
                var text = System.IO.File.ReadAllText(path);
                var allConfigs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(text);
                if (allConfigs != null && allConfigs.TryGetValue(tenantId.ToLowerInvariant(), out var config)) return config;
            }
            catch {}
            return new();
        }
    }

    private static void SaveTenantGateways(string tenantId, Dictionary<string, string> config)
    {
        lock (_gatewayLock)
        {
            try
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "billing");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, "gateways.json");
                Dictionary<string, Dictionary<string, string>> allConfigs = new();
                if (System.IO.File.Exists(path))
                {
                    var text = System.IO.File.ReadAllText(path);
                    allConfigs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(text) ?? new();
                }
                allConfigs[tenantId.ToLowerInvariant()] = config;
                var output = System.Text.Json.JsonSerializer.Serialize(allConfigs);
                System.IO.File.WriteAllText(path, output);
            }
            catch {}
        }
    }

    private static string GetUserAvatar(string email)
    {
        lock (_avatarLock)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles", "avatars.json");
                if (!System.IO.File.Exists(path)) return "";
                var text = System.IO.File.ReadAllText(path);
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(text);
                if (dict != null && dict.TryGetValue(email.ToLowerInvariant(), out var url)) return url;
            }
            catch {}
            return "";
        }
    }

    private static void SaveUserAvatar(string email, string url)
    {
        lock (_avatarLock)
        {
            try
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, "avatars.json");
                Dictionary<string, string> dict = new();
                if (System.IO.File.Exists(path))
                {
                    var text = System.IO.File.ReadAllText(path);
                    dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(text) ?? new();
                }
                dict[email.ToLowerInvariant()] = url;
                var output = System.Text.Json.JsonSerializer.Serialize(dict);
                System.IO.File.WriteAllText(path, output);
            }
            catch {}
        }
    }

    public HomeController(IApplicationDbContext context, IPasswordHasher passwordHasher, IMediator mediator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _mediator = mediator;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Please enter both email and password.";
            return View("Index");
        }

        try
        {
            // Execute LoginUserCommand which handles validation, passwords, tenant context, and seeds refresh token in database
            var authResult = await _mediator.Send(new LoginUserCommand(email, password));

            // Set secure HttpOnly cookies for JWT
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Force secure channel
                SameSite = SameSiteMode.Strict,
                Expires = authResult.Expiration
            };

            Response.Cookies.Append("access_token", authResult.Token, cookieOptions);
            Response.Cookies.Append("refresh_token", authResult.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            // Extract claims from the JWT to ensure absolute parity between Bearer header and Cookie Principal
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(authResult.Token);
            
            var claimsList = jwtToken.Claims.ToList();
            var customAvatar = GetUserAvatar(email);
            if (!string.IsNullOrEmpty(customAvatar))
            {
                var existingAvatar = claimsList.FirstOrDefault(c => c.Type == "AvatarUrl");
                if (existingAvatar != null) claimsList.Remove(existingAvatar);
                claimsList.Add(new Claim("AvatarUrl", customAvatar));
            }
            
            var claimsIdentity = new ClaimsIdentity(claimsList, CookieAuthenticationDefaults.AuthenticationScheme);
            
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = authResult.Expiration
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Authentication failed: {ex.Message}";
            return View("Index");
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        string tenantName, 
        string subdomain, 
        string adminEmail, 
        string adminPassword, 
        string adminFullName)
    {
        if (string.IsNullOrEmpty(tenantName) || 
            string.IsNullOrEmpty(subdomain) || 
            string.IsNullOrEmpty(adminEmail) || 
            string.IsNullOrEmpty(adminPassword) || 
            string.IsNullOrEmpty(adminFullName))
        {
            ViewBag.RegisterError = "All fields are required.";
            return View("Index");
        }

        try
        {
            var tenantId = await _mediator.Send(new RegisterTenantCommand(tenantName, subdomain, adminEmail, adminPassword, adminFullName));

            var authResult = await _mediator.Send(new LoginUserCommand(adminEmail, adminPassword));

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = authResult.Expiration
            };

            Response.Cookies.Append("access_token", authResult.Token, cookieOptions);
            Response.Cookies.Append("refresh_token", authResult.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(authResult.Token);
            var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = authResult.Expiration
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            ViewBag.RegisterError = $"Registration failed: {ex.Message}";
            return View("Index");
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index");
    }

    [HttpGet("landing")]
    public IActionResult Landing()
    {
        return View();
    }

    // Simulated Social Logins
    [HttpGet("auth/google-login")]
    public IActionResult GoogleLogin()
    {
        return RedirectToAction("OAuthPortal", new { provider = "Google" });
    }

    [HttpGet("auth/facebook-login")]
    public IActionResult FacebookLogin()
    {
        return RedirectToAction("OAuthPortal", new { provider = "Facebook" });
    }

    [HttpGet("auth/oauth-portal")]
    public IActionResult OAuthPortal(string provider)
    {
        ViewBag.Provider = provider;
        return View();
    }

    [HttpPost("auth/oauth-callback")]
    public async Task<IActionResult> OAuthCallback(string provider, string email, string fullName, string avatarUrl)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
        {
            return RedirectToAction("Index");
        }

        try
        {
            // 1. Get first available Tenant
            var tenant = await _context.Tenants.FirstOrDefaultAsync();
            if (tenant == null)
            {
                return RedirectToAction("Index");
            }

            // 2. Check if social user exists in database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
            if (user == null)
            {
                // Register new user dynamically in SQL Server
                var dummyPasswordHash = _passwordHasher.HashPassword(Guid.NewGuid().ToString());
                user = new User(tenant.Id, email, dummyPasswordHash, fullName);
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync(default);
            }

            // 3. Authenticate with Cookie Session
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim("AvatarUrl", avatarUrl ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Fetch user roles and add them to claimsIdentity
            var roleIds = await _context.UserRoles
                .IgnoreQueryFilters()
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var roles = await _context.Roles
                .IgnoreQueryFilters()
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();

            foreach (var role in roles)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"OAuth redirection failed: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Index");
        }

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return RedirectToAction("Index");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user == null)
        {
            return RedirectToAction("Index");
        }

        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == user.TenantId);
        
        // Load staff roster dynamic team view
        var employees = await _context.Employees.ToListAsync();
        var users = await _context.Users.ToListAsync();
        
        var teamList = employees.Select(e => {
            var matchingUser = users.FirstOrDefault(u => u.Id == e.UserId);
            return new {
                Id = e.Id,
                FullName = $"{e.FirstName} {e.LastName}",
                FirstName = e.FirstName,
                LastName = e.LastName,
                Department = e.Department,
                IsActive = e.IsActive,
                Email = matchingUser?.Email ?? "staff-member@chaletelite.com"
            };
        }).ToList();

        var gateways = GetTenantGateways(user.TenantId.ToString());

        ViewBag.User = user;
        ViewBag.Tenant = tenant;
        ViewBag.Team = teamList;
        ViewBag.Gateways = gateways;

        return View();
    }

    [HttpPost("profile/update")]
    public async Task<IActionResult> UpdateProfile(string fullName, string email, string tenantName)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Index");
        }

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return RedirectToAction("Index");
        }

        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user != null)
            {
                user.FullName = fullName;
                user.Email = email;

                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == user.TenantId);
                if (tenant != null && !string.IsNullOrEmpty(tenantName))
                {
                    tenant.Name = tenantName;
                }

                await _context.SaveChangesAsync(default);

                // Re-sign in to update cookie claims in real-time
                var avatarUrl = GetUserAvatar(user.Email);
                if (string.IsNullOrEmpty(avatarUrl)) avatarUrl = User.FindFirst("AvatarUrl")?.Value ?? "";
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("TenantId", user.TenantId.ToString()),
                    new Claim("AvatarUrl", avatarUrl)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Fetch user roles and add them to claimsIdentity
                var roleIds = await _context.UserRoles
                    .IgnoreQueryFilters()
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                var roles = await _context.Roles
                    .IgnoreQueryFilters()
                    .Where(r => roleIds.Contains(r.Id))
                    .Select(r => r.Name)
                    .ToListAsync();

                foreach (var role in roles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                }

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                TempData["Success"] = "Profile and settings updated in SQL Server database.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Profile update failed: {ex.Message}";
        }

        return RedirectToAction("Profile");
    }

    [HttpPost("profile/team/create")]
    public async Task<IActionResult> CreateTeamMember(string firstName, string lastName, string department, string email, string role)
    {
        if (User.Identity?.IsAuthenticated != true) return RedirectToAction("Index");

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(department) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
        {
            TempData["Error"] = "All team member fields are required.";
            return RedirectToAction("Profile");
        }

        try
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId)) return RedirectToAction("Index");

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (currentUser == null) return RedirectToAction("Index");

            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            if (exists)
            {
                TempData["Error"] = "A user account with this email address already exists.";
                return RedirectToAction("Profile");
            }

            var defaultPasswordHash = _passwordHasher.HashPassword("StaffPass123!");
            var newUser = new User(currentUser.TenantId, email, defaultPasswordHash, $"{firstName} {lastName}");
            
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(default);

            var roleEntity = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Name == role);
            if (roleEntity != null)
            {
                var userRole = new UserRole(newUser.Id, roleEntity.Id);
                _context.UserRoles.Add(userRole);
            }

            var employee = new Employee(currentUser.TenantId, firstName, lastName, department, newUser.Id);
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync(default);

            TempData["Success"] = $"Team member successfully registered. Default password set to 'StaffPass123!'";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to register staff: {ex.Message}";
        }

        return RedirectToAction("Profile");
    }

    [HttpPost("profile/team/{id:guid}/delete")]
    public async Task<IActionResult> DeleteTeamMember(Guid id)
    {
        if (User.Identity?.IsAuthenticated != true) return RedirectToAction("Index");

        try
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (emp != null)
            {
                if (emp.UserId.HasValue)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == emp.UserId.Value);
                    if (user != null)
                    {
                        user.IsActive = false; // Deactivate account
                    }
                }

                _context.Employees.Remove(emp);
                await _context.SaveChangesAsync(default);
                TempData["Success"] = "Staff member successfully removed from roster.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to remove staff: {ex.Message}";
        }

        return RedirectToAction("Profile");
    }

    [HttpPost("profile/change-password")]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (User.Identity?.IsAuthenticated != true) return RedirectToAction("Index");

        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
        {
            TempData["Error"] = "All password fields are required.";
            return RedirectToAction("Profile");
        }

        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "New password and confirmation do not match.";
            return RedirectToAction("Profile");
        }

        try
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId)) return RedirectToAction("Index");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Index");

            var isValid = _passwordHasher.VerifyPassword(currentPassword, user.PasswordHash);
            if (!isValid)
            {
                TempData["Error"] = "Current password does not match our records.";
                return RedirectToAction("Profile");
            }

            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
            await _context.SaveChangesAsync(default);

            TempData["Success"] = "Password successfully updated in SQL Server.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to update password: {ex.Message}";
        }

        return RedirectToAction("Profile");
    }

    [HttpPost("profile/update-avatar")]
    public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile)
    {
        if (User.Identity?.IsAuthenticated != true) return RedirectToAction("Index");

        if (avatarFile == null || avatarFile.Length == 0)
        {
            TempData["Error"] = "Please select a valid image file.";
            return RedirectToAction("Profile");
        }

        try
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId)) return RedirectToAction("Index");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Index");

            // Validate folder
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var ext = Path.GetExtension(avatarFile.FileName);
            var filename = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(dir, filename);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            var virtualPath = $"/uploads/profiles/{filename}";
            SaveUserAvatar(user.Email, virtualPath);

            // Re-sign in to update cookie claims in real-time
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim("AvatarUrl", virtualPath)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Fetch user roles and add them to claimsIdentity
            var roleIds = await _context.UserRoles
                .IgnoreQueryFilters()
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var roles = await _context.Roles
                .IgnoreQueryFilters()
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();

            foreach (var role in roles)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            TempData["Success"] = "Profile avatar uploaded and synced successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to upload photo: {ex.Message}";
        }

        return RedirectToAction("Profile");
    }

    [HttpPost("profile/payments/save")]
    public async Task<IActionResult> SavePaymentSettings(
        string payPalEnabled, string payPalClientId, string payPalSecret, string payPalEnv,
        string vodafoneEnabled, string vodafoneNumber, string vodafoneMerchantId,
        string stripeEnabled, string stripePublishableKey, string stripeSecretKey,
        string instaPayEnabled, string instaPayAddress,
        string fawryEnabled, string fawryMerchantCode, string fawrySecurityKey)
    {
        if (User.Identity?.IsAuthenticated != true) return RedirectToAction("Index");

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId)) return RedirectToAction("Index");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return RedirectToAction("Index");

        var config = new Dictionary<string, string>
        {
            { "PayPal_Enabled", payPalEnabled ?? "false" },
            { "PayPal_ClientId", payPalClientId ?? "" },
            { "PayPal_Secret", payPalSecret ?? "" },
            { "PayPal_Env", payPalEnv ?? "sandbox" },
            { "Vodafone_Enabled", vodafoneEnabled ?? "false" },
            { "Vodafone_Number", vodafoneNumber ?? "" },
            { "Vodafone_MerchantId", vodafoneMerchantId ?? "" },
            { "Stripe_Enabled", stripeEnabled ?? "false" },
            { "Stripe_PublishableKey", stripePublishableKey ?? "" },
            { "Stripe_SecretKey", stripeSecretKey ?? "" },
            { "InstaPay_Enabled", instaPayEnabled ?? "false" },
            { "InstaPay_Address", instaPayAddress ?? "" },
            { "Fawry_Enabled", fawryEnabled ?? "false" },
            { "Fawry_MerchantCode", fawryMerchantCode ?? "" },
            { "Fawry_SecurityKey", fawrySecurityKey ?? "" }
        };

        SaveTenantGateways(user.TenantId.ToString(), config);

        TempData["Success"] = "Payment gateway integrations updated successfully.";
        return Redirect("/profile#gateways");
    }

    [HttpGet("privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("terms")]
    public IActionResult Terms()
    {
        return View();
    }

    [HttpGet("api-docs")]
    public IActionResult ApiDocs()
    {
        return View();
    }

    [HttpGet("support")]
    public IActionResult Support()
    {
        return View();
    }
}
