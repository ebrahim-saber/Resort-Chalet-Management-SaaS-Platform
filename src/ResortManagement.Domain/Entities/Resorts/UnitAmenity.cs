using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Resorts;

public class UnitAmenity : MustHaveTenantEntityBase
{
    public Guid UnitTypeId { get; set; }
    public string Name { get; set; } = default!;

    private UnitAmenity() { } // EF Core

    public UnitAmenity(Guid tenantId, Guid unitTypeId, string name)
    {
        TenantId = tenantId;
        UnitTypeId = unitTypeId;
        Name = name;
    }
}
