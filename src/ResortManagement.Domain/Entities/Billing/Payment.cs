using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Billing;

public class Payment : MustHaveTenantEntityBase
{
    public Guid InvoiceId { get; set; }
    public string PaymentMethod { get; set; } = default!; // CreditCard, Cash, BankTransfer
    public decimal Amount { get; set; }
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Succeeded, Failed
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    private Payment() { } // EF Core

    public Payment(Guid tenantId, Guid invoiceId, string paymentMethod, decimal amount, string? transactionReference = null)
    {
        TenantId = tenantId;
        InvoiceId = invoiceId;
        PaymentMethod = paymentMethod;
        Amount = amount;
        TransactionReference = transactionReference;
    }

    public void Success() => Status = "Succeeded";
    public void Fail() => Status = "Failed";
}
