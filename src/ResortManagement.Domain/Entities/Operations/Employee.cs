using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Operations;

public class Employee : MustHaveTenantEntityBase
{
    public Guid? UserId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Department { get; set; } = default!; // Housekeeping, Maintenance, FrontOffice
    public bool IsActive { get; set; } = true;

    private Employee() { } // EF Core

    public Employee(Guid tenantId, string firstName, string lastName, string department, Guid? userId = null)
    {
        TenantId = tenantId;
        FirstName = firstName;
        LastName = lastName;
        Department = department;
        UserId = userId;
    }
}
