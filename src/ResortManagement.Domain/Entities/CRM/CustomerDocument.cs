using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.CRM;

public class CustomerDocument : MustHaveTenantEntityBase
{
    public Guid CustomerId { get; set; }
    public string DocumentType { get; set; } = default!; // Passport, NationalID, etc.
    public string DocumentUrl { get; set; } = default!;

    private CustomerDocument() { } // EF Core

    public CustomerDocument(Guid tenantId, Guid customerId, string documentType, string documentUrl)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        DocumentType = documentType;
        DocumentUrl = documentUrl;
    }
}
