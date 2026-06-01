using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Reservations.Events;

public class ReservationCheckedOutEvent : DomainEventBase
{
    public Guid ReservationId { get; }
    public Guid TenantId { get; }
    public Guid UnitId { get; }

    public ReservationCheckedOutEvent(Guid tenantId, Guid reservationId, Guid unitId)
    {
        TenantId = tenantId;
        ReservationId = reservationId;
        UnitId = unitId;
    }
}
