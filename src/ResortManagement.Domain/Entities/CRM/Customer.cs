using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.CRM;

public class Customer : AggregateRoot
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Email { get; set; }
    public string Phone { get; set; } = default!;
    public string? IdentityNumber { get; set; }
    public string? Nationality { get; set; }

    private Customer() { } // EF Core

    public Customer(Guid tenantId, string firstName, string lastName, string? email, string phone, string? identityNumber = null, string? nationality = null)
    {
        TenantId = tenantId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        IdentityNumber = identityNumber;
        Nationality = nationality;
    }
}
