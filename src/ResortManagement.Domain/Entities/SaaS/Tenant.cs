using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.SaaS;

public class Tenant : EntityBase
{
    public string Name { get; set; } = default!;
    public string Subdomain { get; set; } = default!;
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; } = true;

    // Brand Configurations
    public string? PrimaryColor { get; set; } = "#cd9a5f";
    public string? SecondaryColor { get; set; } = "#0ea5e9";
    public string? DarkBgColor { get; set; } = "#030712";
    public string? SurfaceColor { get; set; } = "#0b0f19";
    public string? HeroBannerUrl { get; set; } = "https://images.unsplash.com/photo-1542601906990-b4d3fb778b09?q=80&w=2013&auto=format&fit=crop";
    public string? WelcomeMessageEn { get; set; } = "Find Your Ultimate Mountain Hideaway";
    public string? WelcomeMessageAr { get; set; } = "ابحث عن ملاذك الجبلي المثالي";

    private Tenant() { } // EF Core

    public Tenant(string name, string subdomain, string? logoPath = null)
    {
        Name = name;
        Subdomain = subdomain.ToLower().Trim();
        LogoPath = logoPath;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
