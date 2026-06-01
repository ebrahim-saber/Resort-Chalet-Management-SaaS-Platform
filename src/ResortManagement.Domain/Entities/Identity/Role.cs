using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Identity;

public class Role : EntityBase
{
    public string Name { get; set; } = default!;
    public Guid? TenantId { get; set; } // Null for system-wide default roles, Guid for tenant custom roles

    private Role() { } // EF Core

    public Role(string name, Guid? tenantId = null)
    {
        Name = name;
        TenantId = tenantId;
    }
}
