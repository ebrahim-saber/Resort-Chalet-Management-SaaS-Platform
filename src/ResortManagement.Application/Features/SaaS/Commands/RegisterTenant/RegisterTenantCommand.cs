using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Domain.Entities.SaaS;
using ResortManagement.Domain.Entities.Identity;

namespace ResortManagement.Application.Features.SaaS.Commands.RegisterTenant;

public record RegisterTenantCommand(
    string TenantName,
    string Subdomain,
    string AdminEmail,
    string AdminPassword,
    string AdminFullName
) : IRequest<Guid>;

public class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterTenantCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        var subdomainNormalized = request.Subdomain.ToLower().Trim();
        var emailNormalized = request.AdminEmail.ToLower().Trim();

        // 1. Verify subdomain is unique
        var subdomainExists = await _context.Tenants
            .AnyAsync(t => t.Subdomain == subdomainNormalized, cancellationToken);

        if (subdomainExists)
        {
            throw new ValidationException(new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "Subdomain", new[] { "This subdomain is already taken." } }
            });
        }

        // 2. Verify email is globally unique
        var emailExists = await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == emailNormalized, cancellationToken);

        if (emailExists)
        {
            throw new ValidationException(new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "AdminEmail", new[] { "A user with this email address already exists." } }
            });
        }

        // 3. Create Tenant
        var tenant = new Tenant(request.TenantName, subdomainNormalized);
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken); // Generates tenant.Id

        // 4. Create Tenant Admin Role
        var adminRole = new Role("TenantAdmin", tenant.Id);
        _context.Roles.Add(adminRole);
        await _context.SaveChangesAsync(cancellationToken); // Generates adminRole.Id

        // 5. Hash Admin password and create Admin User
        var adminPasswordHash = _passwordHasher.HashPassword(request.AdminPassword);
        var adminUser = new User(tenant.Id, emailNormalized, adminPasswordHash, request.AdminFullName);
        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync(cancellationToken); // Generates adminUser.Id

        // 6. Bind Admin User to Admin Role
        var userRole = new UserRole(adminUser.Id, adminRole.Id);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}
