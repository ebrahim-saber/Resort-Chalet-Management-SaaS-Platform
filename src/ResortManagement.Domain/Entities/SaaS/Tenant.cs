using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.SaaS;

public class Tenant : EntityBase
{
    public string Name { get; set; } = default!;
    public string Subdomain { get; set; } = default!;
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; } = true;

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
