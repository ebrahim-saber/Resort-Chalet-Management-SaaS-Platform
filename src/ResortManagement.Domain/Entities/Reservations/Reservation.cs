using System;
using ResortManagement.Domain.Common;
using ResortManagement.Domain.Entities.Reservations.Events;

namespace ResortManagement.Domain.Entities.Reservations;

public class Reservation : AggregateRoot
{
    public Guid CustomerId { get; set; }
    public Guid UnitId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Pending, Confirmed, CheckedIn, CheckedOut, Cancelled
    public decimal TotalPrice { get; set; }

    private Reservation() { } // EF Core

    public Reservation(Guid tenantId, Guid customerId, Guid unitId, DateTime checkInDate, DateTime checkOutDate, decimal totalPrice)
    {
        if (checkOutDate <= checkInDate)
        {
            throw new ArgumentException("Check-out date must be after check-in date.");
        }

        TenantId = tenantId;
        CustomerId = customerId;
        UnitId = unitId;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        TotalPrice = totalPrice;

        AddDomainEvent(new ReservationCreatedEvent(tenantId, Id, totalPrice));
    }

    public void Confirm()
    {
        if (Status != "Draft" && Status != "Pending")
        {
            throw new InvalidOperationException("Only Draft or Pending reservations can be confirmed.");
        }
        Status = "Confirmed";
    }

    public void CheckIn()
    {
        if (Status != "Confirmed")
        {
            throw new InvalidOperationException("Only Confirmed reservations can be checked in.");
        }
        Status = "CheckedIn";
    }

    public void CheckOut()
    {
        if (Status != "CheckedIn")
        {
            throw new InvalidOperationException("Only CheckedIn reservations can be checked out.");
        }
        Status = "CheckedOut";
        AddDomainEvent(new ReservationCheckedOutEvent(TenantId, Id, UnitId));
    }

    public void Cancel()
    {
        if (Status == "CheckedIn" || Status == "CheckedOut")
        {
            throw new InvalidOperationException("Cannot cancel an active or completed stay.");
        }
        Status = "Cancelled";
    }
}
