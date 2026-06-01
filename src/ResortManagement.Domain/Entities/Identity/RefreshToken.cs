using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Identity;

public class RefreshToken : MustHaveTenantEntityBase
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime? Revoked { get; set; }

    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsActive => Revoked == null && !IsExpired;

    private RefreshToken() { } // EF Core

    public RefreshToken(Guid tenantId, Guid userId, string token, DateTime expires)
    {
        TenantId = tenantId;
        UserId = userId;
        Token = token;
        Expires = expires;
    }

    public void Revoke()
    {
        Revoked = DateTime.UtcNow;
    }
}
