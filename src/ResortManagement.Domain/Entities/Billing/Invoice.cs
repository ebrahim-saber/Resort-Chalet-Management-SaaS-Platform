using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Billing;

public class Invoice : AggregateRoot
{
    public Guid ReservationId { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Unpaid"; // Unpaid, Paid, PartiallyPaid, Cancelled

    private Invoice() { } // EF Core

    public Invoice(Guid tenantId, Guid reservationId, string invoiceNumber, decimal totalAmount, DateTime dueDate)
    {
        TenantId = tenantId;
        ReservationId = reservationId;
        InvoiceNumber = invoiceNumber;
        TotalAmount = totalAmount;
        DueDate = dueDate;
    }

    public void MarkPaid() => Status = "Paid";
    public void MarkPartiallyPaid() => Status = "PartiallyPaid";
    public void Cancel() => Status = "Cancelled";
}
