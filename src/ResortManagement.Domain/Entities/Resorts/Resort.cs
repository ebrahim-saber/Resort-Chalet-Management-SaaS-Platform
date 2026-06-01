using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Resorts;

public class Resort : AggregateRoot
{
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string? ContactNumber { get; set; }

    private Resort() { } // EF Core

    public Resort(Guid tenantId, string name, string address, string? contactNumber = null)
    {
        TenantId = tenantId;
        Name = name;
        Address = address;
        ContactNumber = contactNumber;
    }
}
