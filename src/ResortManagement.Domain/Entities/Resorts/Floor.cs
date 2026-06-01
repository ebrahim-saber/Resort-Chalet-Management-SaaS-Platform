using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Resorts;

public class Floor : MustHaveTenantEntityBase
{
    public Guid BuildingId { get; set; }
    public int FloorNumber { get; set; }

    private Floor() { } // EF Core

    public Floor(Guid tenantId, Guid buildingId, int floorNumber)
    {
        TenantId = tenantId;
        BuildingId = buildingId;
        FloorNumber = floorNumber;
    }
}
