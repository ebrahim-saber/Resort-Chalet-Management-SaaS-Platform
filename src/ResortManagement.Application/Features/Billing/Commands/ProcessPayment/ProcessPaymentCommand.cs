using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Domain.Entities.Billing;
using ResortManagement.Domain.Entities.Reservations;

namespace ResortManagement.Application.Features.Billing.Commands.ProcessPayment;

public record ProcessPaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    string PaymentMethod,
    string? TransactionReference
) : IRequest<Guid>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ProcessPaymentCommandHandler(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;

        // 1. Retrieve Invoice
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);
        }

        if (invoice.Status == "Paid")
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "InvoiceId", new[] { "This invoice has already been fully paid." } }
            });
        }

        if (invoice.Status == "Cancelled")
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "InvoiceId", new[] { "Cannot pay a cancelled invoice." } }
            });
        }

        // 2. Create Payment record
        var payment = new Payment(
            tenantId,
            invoice.Id,
            request.PaymentMethod,
            request.Amount,
            request.TransactionReference
        );

        // Simulated external payment processing - assume successful
        payment.Success();
        _context.Payments.Add(payment);

        // 3. Update Invoice and Reservation states based on total paid amount
        var pastSuccessfulPayments = await _context.Payments
            .Where(p => p.InvoiceId == invoice.Id && p.Status == "Succeeded")
            .SumAsync(p => p.Amount, cancellationToken);

        var totalPaid = pastSuccessfulPayments + request.Amount;

        if (totalPaid >= invoice.TotalAmount)
        {
            invoice.MarkPaid();

            // Find linked reservation and transition to Confirmed
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == invoice.ReservationId, cancellationToken);
            if (reservation != null && (reservation.Status == "Draft" || reservation.Status == "Pending"))
            {
                var oldStatus = reservation.Status;
                reservation.Confirm();

                var history = new ReservationHistory(
                    tenantId,
                    reservation.Id,
                    oldStatus,
                    "Confirmed",
                    Guid.Empty, // In production, resolved from ICurrentUserService
                    "Reservation confirmed automatically upon full invoice payment."
                );
                _context.ReservationHistories.Add(history);
            }
        }
        else
        {
            invoice.MarkPartiallyPaid();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return payment.Id;
    }
}
