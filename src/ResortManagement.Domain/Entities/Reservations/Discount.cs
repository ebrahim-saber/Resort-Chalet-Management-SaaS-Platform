using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Reservations;

public class Discount : MustHaveTenantEntityBase
{
    public string Code { get; set; } = default!;
    public decimal Value { get; set; }
    public string DiscountType { get; set; } = default!; // Percentage, FlatAmount
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    private Discount() { } // EF Core

    public Discount(Guid tenantId, string code, decimal value, string discountType, DateTime startDate, DateTime endDate)
    {
        TenantId = tenantId;
        Code = code.ToUpper().Trim();
        Value = value;
        DiscountType = discountType;
        StartDate = startDate;
        EndDate = endDate;
    }
}
