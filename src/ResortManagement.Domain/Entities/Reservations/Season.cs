using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Reservations;

public class Season : MustHaveTenantEntityBase
{
    public string Name { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PriceMultiplier { get; set; }

    private Season() { } // EF Core

    public Season(Guid tenantId, string name, DateTime startDate, DateTime endDate, decimal priceMultiplier)
    {
        TenantId = tenantId;
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        PriceMultiplier = priceMultiplier;
    }
}
