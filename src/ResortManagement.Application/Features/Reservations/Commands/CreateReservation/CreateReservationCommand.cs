using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Domain.Entities.Reservations;
using ResortManagement.Domain.Entities.Resorts;
using UnitEntity = ResortManagement.Domain.Entities.Resorts.Unit;

namespace ResortManagement.Application.Features.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(
    Guid CustomerId,
    Guid UnitId,
    DateTime CheckInDate,
    DateTime CheckOutDate
) : IRequest<Guid>;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CreateReservationCommandHandler(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        var checkIn = request.CheckInDate.Date;
        var checkOut = request.CheckOutDate.Date;

        // 1. Verify Unit exists
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == request.UnitId, cancellationToken);
        if (unit == null || !unit.IsActive)
        {
            throw new NotFoundException(nameof(UnitEntity), request.UnitId);
        }

        // 2. Validate availability (prevent double-booking!)
        var isOccupied = await _context.Reservations
            .AnyAsync(r => r.UnitId == request.UnitId && r.CheckInDate < checkOut && r.CheckOutDate > checkIn && r.Status != "Cancelled", cancellationToken);

        if (isOccupied)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "UnitId", new[] { "This room/unit is already booked for the selected dates." } }
            });
        }

        // 3. Retrieve unit type for pricing
        var unitType = await _context.UnitTypes
            .FirstOrDefaultAsync(ut => ut.Id == unit.UnitTypeId, cancellationToken);
        if (unitType == null)
        {
            throw new NotFoundException(nameof(UnitType), unit.UnitTypeId);
        }

        // 4. Run Dynamic Pricing Engine
        var seasons = await _context.Seasons
            .Where(s => checkIn < s.EndDate && checkOut > s.StartDate)
            .ToListAsync(cancellationToken);

        var pricingRules = await _context.PricingRules
            .Where(pr => pr.RuleType == "Weekend" || (pr.SpecificDate >= checkIn && pr.SpecificDate < checkOut))
            .ToListAsync(cancellationToken);

        decimal totalPrice = 0;
        for (var date = checkIn; date < checkOut; date = date.AddDays(1))
        {
            decimal nightPrice = unitType.BasePrice;

            var activeSeason = seasons.FirstOrDefault(s => date >= s.StartDate.Date && date <= s.EndDate.Date);
            if (activeSeason != null)
            {
                nightPrice *= activeSeason.PriceMultiplier;
            }

            if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
            {
                var weekendRule = pricingRules.FirstOrDefault(pr => pr.RuleType == "Weekend");
                if (weekendRule != null)
                {
                    if (weekendRule.Multiplier.HasValue)
                    {
                        nightPrice *= weekendRule.Multiplier.Value;
                    }
                    else if (weekendRule.FlatAmount.HasValue)
                    {
                        nightPrice += weekendRule.FlatAmount.Value;
                    }
                }
            }

            var holidayRule = pricingRules.FirstOrDefault(pr => pr.SpecificDate.HasValue && pr.SpecificDate.Value.Date == date);
            if (holidayRule != null)
            {
                if (holidayRule.Multiplier.HasValue)
                {
                    nightPrice *= holidayRule.Multiplier.Value;
                }
                else if (holidayRule.FlatAmount.HasValue)
                {
                    nightPrice += holidayRule.FlatAmount.Value;
                }
            }

            totalPrice += nightPrice;
        }

        // 5. Save Reservation Aggregate
        var reservation = new Reservation(tenantId, request.CustomerId, request.UnitId, checkIn, checkOut, totalPrice);
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken); // Generates reservation.Id

        // 6. Log History Entry
        var currentUserId = Guid.Empty; // In production, resolved from ICurrentUserService
        var history = new ReservationHistory(tenantId, reservation.Id, "None", "Draft", currentUserId, "Reservation created dynamically.");
        _context.ReservationHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
