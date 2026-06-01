using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Resorts;

public class Unit : AggregateRoot
{
    public Guid FloorId { get; set; }
    public Guid UnitTypeId { get; set; }
    public string UnitNumber { get; set; } = default!;
    public string HousekeepingStatus { get; set; } = "Clean"; // Clean, Dirty, InProgress, OutOfService
    public bool IsActive { get; set; } = true;

    private Unit() { } // EF Core

    public Unit(Guid tenantId, Guid floorId, Guid unitTypeId, string unitNumber)
    {
        TenantId = tenantId;
        FloorId = floorId;
        UnitTypeId = unitTypeId;
        UnitNumber = unitNumber;
    }

    public void MarkDirty() => HousekeepingStatus = "Dirty";
    public void MarkClean() => HousekeepingStatus = "Clean";
    public void StartCleaning() => HousekeepingStatus = "InProgress";
    public void BlockForMaintenance() => HousekeepingStatus = "OutOfService";
}
