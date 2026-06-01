using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.SaaS;

public class SubscriptionPlan : EntityBase
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int MaxResorts { get; set; }
    public int MaxUnits { get; set; }
    public string? FeaturesJson { get; set; }

    private SubscriptionPlan() { } // EF Core

    public SubscriptionPlan(string name, decimal price, int maxResorts, int maxUnits, string? featuresJson = null)
    {
        Name = name;
        Price = price;
        MaxResorts = maxResorts;
        MaxUnits = maxUnits;
        FeaturesJson = featuresJson;
    }
}
