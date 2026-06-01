using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Features.Reservations.DTOs;
using ResortManagement.Domain.Entities.Reservations;

namespace ResortManagement.Application.Features.Reservations.Queries.GetAvailableUnits;

public record GetAvailableUnitsQuery(
    DateTime CheckInDate,
    DateTime CheckOutDate,
    Guid ResortId
) : IRequest<List<AvailableUnitDto>>;

public class GetAvailableUnitsQueryHandler : IRequestHandler<GetAvailableUnitsQuery, List<AvailableUnitDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableUnitsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableUnitDto>> Handle(GetAvailableUnitsQuery request, CancellationToken cancellationToken)
    {
        var checkIn = request.CheckInDate.Date;
        var checkOut = request.CheckOutDate.Date;

        // 1. Fetch active pricing multipliers/rules & seasons
        var seasons = await _context.Seasons
            .Where(s => checkIn < s.EndDate && checkOut > s.StartDate)
            .ToListAsync(cancellationToken);

        var pricingRules = await _context.PricingRules
            .Where(pr => pr.RuleType == "Weekend" || (pr.SpecificDate >= checkIn && pr.SpecificDate < checkOut))
            .ToListAsync(cancellationToken);

        // 2. Fetch occupied unit IDs during the range (Status is not Cancelled)
        var occupiedUnitIds = await _context.Reservations
            .Where(r => r.CheckInDate < checkOut && r.CheckOutDate > checkIn && r.Status != "Cancelled")
            .Select(r => r.UnitId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 3. Query all units for the target resort
        var unitsQuery = from unit in _context.Units
                         join floor in _context.Floors on unit.FloorId equals floor.Id
                         join building in _context.Buildings on floor.BuildingId equals building.Id
                         join unitType in _context.UnitTypes on unit.UnitTypeId equals unitType.Id
                         where building.ResortId == request.ResortId && unit.IsActive && !occupiedUnitIds.Contains(unit.Id)
                         select new
                         {
                             UnitId = unit.Id,
                             UnitNumber = unit.UnitNumber,
                             UnitTypeId = unitType.Id,
                             UnitTypeName = unitType.Name,
                             BasePrice = unitType.BasePrice,
                             MaxOccupancy = unitType.MaxOccupancy
                         };

        var availableUnits = await unitsQuery.ToListAsync(cancellationToken);
        var resultList = new List<AvailableUnitDto>();

        // 4. Calculate dynamic pricing for each available unit
        foreach (var item in availableUnits)
        {
            decimal totalCalculatedPrice = 0;

            // Iterate night-by-night
            for (var date = checkIn; date < checkOut; date = date.AddDays(1))
            {
                decimal nightPrice = item.BasePrice;

                // A. Apply Seasonal Multiplier if matches
                var activeSeason = seasons.FirstOrDefault(s => date >= s.StartDate.Date && date <= s.EndDate.Date);
                if (activeSeason != null)
                {
                    nightPrice *= activeSeason.PriceMultiplier;
                }

                // B. Apply Weekend Markup if day is Friday or Saturday
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

                // C. Apply specific date Holiday Rules
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

                totalCalculatedPrice += nightPrice;
            }

            resultList.Add(new AvailableUnitDto(
                item.UnitId,
                item.UnitNumber,
                item.UnitTypeId,
                item.UnitTypeName,
                item.BasePrice,
                totalCalculatedPrice,
                item.MaxOccupancy
            ));
        }

        return resultList;
    }
}
