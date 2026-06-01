using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Resorts;

public class Building : MustHaveTenantEntityBase
{
    public Guid ResortId { get; set; }
    public string Name { get; set; } = default!;

    private Building() { } // EF Core

    public Building(Guid tenantId, Guid resortId, string name)
    {
        TenantId = tenantId;
        ResortId = resortId;
        Name = name;
    }
}
