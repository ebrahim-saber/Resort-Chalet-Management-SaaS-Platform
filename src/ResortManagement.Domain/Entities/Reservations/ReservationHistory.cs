using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Reservations;

public class ReservationHistory : MustHaveTenantEntityBase
{
    public Guid ReservationId { get; set; }
    public string FromStatus { get; set; } = default!;
    public string ToStatus { get; set; } = default!;
    public Guid ChangedById { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    private ReservationHistory() { } // EF Core

    public ReservationHistory(Guid tenantId, Guid reservationId, string fromStatus, string toStatus, Guid changedById, string? notes = null)
    {
        TenantId = tenantId;
        ReservationId = reservationId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedById = changedById;
        Notes = notes;
    }
}
