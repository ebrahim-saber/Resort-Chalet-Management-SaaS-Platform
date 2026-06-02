using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Application.Features.Identity.DTOs;
using ResortManagement.Domain.Entities.Identity;

namespace ResortManagement.Application.Features.Identity.Commands.LoginUser;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<AuthResponseDto>;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginUserCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var emailNormalized = request.Email.ToLower().Trim();

        // 1. Fetch user globally ignoring tenant query filter since at login phase tenant context is unknown!
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == emailNormalized, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Auth", new[] { "Invalid email address or password." } }
            });
        }

        // 2. Verify password hash
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Auth", new[] { "Invalid email address or password." } }
            });
        }

        // 3. Resolve permissions mapped to User -> UserRoles -> Roles -> RolePermissions -> Permissions
        var roleIds = await _context.UserRoles
            .IgnoreQueryFilters() // Roles can be shared or tenant-specific
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        var permissions = await _context.RolePermissions
            .IgnoreQueryFilters()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Join(_context.Permissions.IgnoreQueryFilters(),
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var roles = await _context.Roles
            .IgnoreQueryFilters()
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        // 4. Generate JWT Access Token
        var token = _jwtProvider.GenerateToken(user, permissions, roles);

        // 5. Generate and Save Refresh Token
        var refreshTokenString = _jwtProvider.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Refresh token lasts 7 days
        var refreshToken = new RefreshToken(user.TenantId, user.Id, refreshTokenString, refreshTokenExpiry);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(token, refreshTokenString, DateTime.UtcNow.AddHours(1));
    }
}
