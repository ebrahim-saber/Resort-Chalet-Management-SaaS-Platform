using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Resorts;

public class UnitImage : MustHaveTenantEntityBase
{
    public Guid UnitTypeId { get; set; }
    public string ImageUrl { get; set; } = default!;
    public bool IsPrimary { get; set; }

    private UnitImage() { } // EF Core

    public UnitImage(Guid tenantId, Guid unitTypeId, string imageUrl, bool isPrimary = false)
    {
        TenantId = tenantId;
        UnitTypeId = unitTypeId;
        ImageUrl = imageUrl;
        IsPrimary = isPrimary;
    }
}
