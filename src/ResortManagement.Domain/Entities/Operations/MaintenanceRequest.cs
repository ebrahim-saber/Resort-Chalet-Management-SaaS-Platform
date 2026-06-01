using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Operations;

public class MaintenanceRequest : AggregateRoot
{
    public Guid UnitId { get; set; }
    public Guid RequestedById { get; set; } // UserId
    public Guid? AssignedToId { get; set; } // EmployeeId
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Status { get; set; } = "Open"; // Open, Assigned, InProgress, Resolved, Closed

    private MaintenanceRequest() { } // EF Core

    public MaintenanceRequest(Guid tenantId, Guid unitId, Guid requestedById, string title, string description, string priority)
    {
        TenantId = tenantId;
        UnitId = unitId;
        RequestedById = requestedById;
        Title = title;
        Description = description;
        Priority = priority;
    }

    public void AssignTo(Guid employeeId)
    {
        AssignedToId = employeeId;
        Status = "Assigned";
    }

    public void StartRepair() => Status = "InProgress";
    public void Resolve() => Status = "Resolved";
    public void CloseRequest() => Status = "Closed";
}
