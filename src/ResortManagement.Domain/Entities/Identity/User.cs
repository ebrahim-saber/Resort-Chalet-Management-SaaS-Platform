using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Identity;

public class User : AggregateRoot
{
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    private User() { } // EF Core

    public User(Guid tenantId, string email, string passwordHash, string fullName)
    {
        TenantId = tenantId;
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
    }
}
