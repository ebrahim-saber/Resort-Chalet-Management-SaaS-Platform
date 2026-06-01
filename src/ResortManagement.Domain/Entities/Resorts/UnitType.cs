using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Resorts;

public class UnitType : MustHaveTenantEntityBase
{
    public Guid ResortId { get; set; }
    public string Name { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public int MaxOccupancy { get; set; }

    private UnitType() { } // EF Core

    public UnitType(Guid tenantId, Guid resortId, string name, decimal basePrice, int maxOccupancy)
    {
        TenantId = tenantId;
        ResortId = resortId;
        Name = name;
        BasePrice = basePrice;
        MaxOccupancy = maxOccupancy;
    }
}
