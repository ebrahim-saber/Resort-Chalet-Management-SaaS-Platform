using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Operations;
using ResortManagement.Domain.Entities.Reservations.Events;

namespace ResortManagement.Application.Features.Reservations.EventHandlers;

public class ReservationCreatedNotificationHandler : INotificationHandler<ReservationCreatedEvent>
{
    private readonly IApplicationDbContext _context;

    public ReservationCreatedNotificationHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReservationCreatedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Retrieve the reservation and customer details
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == notification.ReservationId, cancellationToken);

        if (reservation == null) return;

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == reservation.CustomerId, cancellationToken);

        if (customer == null) return;

        var recipientEmail = customer.Email ?? "customer@example.com";

        // 2. Create the notification record in the database
        var dbNotification = new Notification(
            notification.TenantId,
            null,
            "Email",
            recipientEmail,
            "Reservation Created Successfully",
            $"Dear {customer.FirstName} {customer.LastName},\n\nYour reservation has been received. Your total amount is {notification.TotalAmount:C}.\n\nPlease review your invoice and proceed with the payment to confirm your stay.\n\nThank you for choosing us!"
        );

        _context.Notifications.Add(dbNotification);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
