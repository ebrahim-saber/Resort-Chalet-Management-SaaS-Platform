using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Billing;
using ResortManagement.Domain.Entities.Reservations.Events;

namespace ResortManagement.Application.Features.Reservations.EventHandlers;

public class ReservationCreatedEventHandler : INotificationHandler<ReservationCreatedEvent>
{
    private readonly IApplicationDbContext _context;

    public ReservationCreatedEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReservationCreatedEvent notification, CancellationToken cancellationToken)
    {
        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{notification.ReservationId.ToString()[..8].ToUpper()}";
        var dueDate = DateTime.UtcNow.AddDays(1); // 24 hours due date

        var invoice = new Invoice(
            notification.TenantId,
            notification.ReservationId,
            invoiceNumber,
            notification.TotalAmount,
            dueDate
        );

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
