using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Reservations;

public class PricingRule : MustHaveTenantEntityBase
{
    public string Name { get; set; } = default!;
    public string RuleType { get; set; } = default!; // Weekend, Holiday, Custom
    public decimal? Multiplier { get; set; }
    public decimal? FlatAmount { get; set; }
    public DateTime? SpecificDate { get; set; }

    private PricingRule() { } // EF Core

    public PricingRule(Guid tenantId, string name, string ruleType, decimal? multiplier, decimal? flatAmount = null, DateTime? specificDate = null)
    {
        TenantId = tenantId;
        Name = name;
        RuleType = ruleType;
        Multiplier = multiplier;
        FlatAmount = flatAmount;
        SpecificDate = specificDate;
    }
}
