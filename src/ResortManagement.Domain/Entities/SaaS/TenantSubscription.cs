using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.SaaS;

public class TenantSubscription : MustHaveTenantEntityBase
{
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Suspended, Expired

    private TenantSubscription() { } // EF Core

    public TenantSubscription(Guid tenantId, Guid planId, DateTime startDate, DateTime endDate)
    {
        TenantId = tenantId;
        PlanId = planId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void Suspend() => Status = "Suspended";
    public void Reactivate() => Status = "Active";
}
