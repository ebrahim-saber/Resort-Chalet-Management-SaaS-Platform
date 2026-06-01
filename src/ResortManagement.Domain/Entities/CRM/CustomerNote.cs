using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.CRM;

public class CustomerNote : MustHaveTenantEntityBase
{
    public Guid CustomerId { get; set; }
    public string Note { get; set; } = default!;
    public Guid AuthorId { get; set; }

    private CustomerNote() { } // EF Core

    public CustomerNote(Guid tenantId, Guid customerId, string note, Guid authorId)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        Note = note;
        AuthorId = authorId;
    }
}
