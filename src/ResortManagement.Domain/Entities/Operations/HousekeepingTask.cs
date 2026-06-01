using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Operations;

public class HousekeepingTask : MustHaveTenantEntityBase
{
    public Guid UnitId { get; set; }
    public Guid? AssignedToId { get; set; } // EmployeeId
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed
    public string? Notes { get; set; }

    private HousekeepingTask() { } // EF Core

    public HousekeepingTask(Guid tenantId, Guid unitId, Guid? assignedToId = null, string? notes = null)
    {
        TenantId = tenantId;
        UnitId = unitId;
        AssignedToId = assignedToId;
        Notes = notes;
    }

    public void StartTask() => Status = "InProgress";
    public void CompleteTask() => Status = "Completed";
}
