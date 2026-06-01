using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Reservations.Events;

public class ReservationCreatedEvent : DomainEventBase
{
    public Guid ReservationId { get; }
    public Guid TenantId { get; }
    public decimal TotalAmount { get; }

    public ReservationCreatedEvent(Guid tenantId, Guid reservationId, decimal totalAmount)
    {
        TenantId = tenantId;
        ReservationId = reservationId;
        TotalAmount = totalAmount;
    }
}
