using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Domain.Entities.Identity;

namespace ResortManagement.Application.Features.Identity.Commands.RegisterUser;

public record RegisterUserCommand(
    Guid TenantId,
    string Email,
    string Password,
    string FullName
) : IRequest<Guid>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if user already exists
        var emailNormalized = request.Email.ToLower().Trim();
        var emailExists = await _context.Users
            .IgnoreQueryFilters() // Ignore tenant global queries when checking global email unique checks!
            .AnyAsync(u => u.Email == emailNormalized, cancellationToken);

        if (emailExists)
        {
            throw new ValidationException(new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "Email", new[] { "A user with this email address already exists." } }
            });
        }

        // 2. Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // 3. Create user
        var user = new User(request.TenantId, emailNormalized, passwordHash, request.FullName);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
