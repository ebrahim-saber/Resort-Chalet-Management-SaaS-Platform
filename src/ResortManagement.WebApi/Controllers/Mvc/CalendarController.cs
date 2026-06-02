using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Reservations;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Authorize(Roles = "Administrator,Operations Manager,Receptionist")]
[Route("calendar")]
public class CalendarController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CalendarController(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public class CalendarUnitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string HousekeepingStatus { get; set; } = null!;
        public string BuildingName { get; set; } = null!;
    }

    public class CalendarReservationDto
    {
        public Guid Id { get; set; }
        public Guid UnitId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string GuestName { get; set; } = null!;
    }

    public class CalendarHousekeepingDto
    {
        public Guid UnitId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CalendarMaintenanceDto
    {
        public Guid UnitId { get; set; }
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(Guid? resortId, DateTime? startDate)
    {
        var tenantId = _tenantProvider.TenantId;

        // Resolve active resort
        var resorts = await _context.Resorts.ToListAsync();
        var selectedResortId = resortId ?? resorts.FirstOrDefault()?.Id ?? Guid.Empty;
        var selectedResort = resorts.FirstOrDefault(r => r.Id == selectedResortId);

        // Date boundaries (30-day timeline)
        var start = (startDate ?? DateTime.Today).Date;
        var end = start.AddDays(30);

        // Fetch units for selected resort using manual joins
        var units = await (from u in _context.Units
                           join f in _context.Floors on u.FloorId equals f.Id
                           join b in _context.Buildings on f.BuildingId equals b.Id
                           where b.ResortId == selectedResortId && !u.IsDeleted
                           orderby b.Name, u.UnitNumber
                           select new CalendarUnitDto
                           {
                               Id = u.Id,
                               Name = u.UnitNumber,
                               HousekeepingStatus = u.HousekeepingStatus,
                               BuildingName = b.Name
                           }).ToListAsync();

        var unitIds = units.Select(u => u.Id).ToList();

        // Fetch reservations in range using manual joins
        var reservations = await (from r in _context.Reservations
                                  join c in _context.Customers on r.CustomerId equals c.Id
                                  where unitIds.Contains(r.UnitId) && r.Status != "Cancelled" && r.CheckInDate < end && r.CheckOutDate > start && !r.IsDeleted
                                  select new CalendarReservationDto
                                  {
                                      Id = r.Id,
                                      UnitId = r.UnitId,
                                      CheckInDate = r.CheckInDate,
                                      CheckOutDate = r.CheckOutDate,
                                      TotalPrice = r.TotalPrice,
                                      GuestName = c.FirstName + " " + c.LastName
                                  }).ToListAsync();

        // Fetch housekeeping tasks in range
        var housekeeping = await _context.HousekeepingTasks
            .Where(h => unitIds.Contains(h.UnitId) && h.CreatedAt >= start && h.CreatedAt < end && !h.IsDeleted)
            .Select(h => new CalendarHousekeepingDto
            {
                UnitId = h.UnitId,
                Status = h.Status,
                CreatedAt = h.CreatedAt
            })
            .ToListAsync();

        // Fetch maintenance requests in range
        var maintenance = await _context.MaintenanceRequests
            .Where(m => unitIds.Contains(m.UnitId) && m.CreatedAt >= start && m.CreatedAt < end && !m.IsDeleted)
            .Select(m => new CalendarMaintenanceDto
            {
                UnitId = m.UnitId,
                Description = m.Description,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        ViewBag.Resorts = resorts;
        ViewBag.SelectedResortId = selectedResortId;
        ViewBag.SelectedResortName = selectedResort?.Name ?? "Select Resort";
        ViewBag.StartDate = start;
        ViewBag.EndDate = end;
        ViewBag.Units = units;
        ViewBag.Reservations = reservations;
        ViewBag.Housekeeping = housekeeping;
        ViewBag.Maintenance = maintenance;

        return View();
    }

    [HttpPost("update-booking")]
    public async Task<IActionResult> UpdateBookingDates(Guid reservationId, DateTime checkIn, DateTime checkOut, Guid unitId)
    {
        try
        {
            var booking = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (booking == null)
            {
                return Json(new { success = false, message = "Reservation not found." });
            }

            // Validate double-booking overlap
            var isOverlapping = await _context.Reservations
                .AnyAsync(r => r.Id != reservationId && r.UnitId == unitId && r.CheckInDate < checkOut && r.CheckOutDate > checkIn && r.Status != "Cancelled");

            if (isOverlapping)
            {
                return Json(new { success = false, message = "The selected unit is already occupied during these dates." });
            }

            // Recalculate price dynamically
            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId);
            if (unit == null)
            {
                return Json(new { success = false, message = "Unit not found." });
            }

            var unitType = await _context.UnitTypes.FirstOrDefaultAsync(ut => ut.Id == unit.UnitTypeId);
            if (unitType == null)
            {
                return Json(new { success = false, message = "Unit type not found." });
            }

            // Simple seasonal pricing calculation
            var seasons = await _context.Seasons
                .Where(s => checkIn < s.EndDate && checkOut > s.StartDate)
                .ToListAsync();

            decimal totalPrice = 0;
            for (var date = checkIn.Date; date < checkOut.Date; date = date.AddDays(1))
            {
                decimal nightPrice = unitType.BasePrice;
                var activeSeason = seasons.FirstOrDefault(s => date >= s.StartDate.Date && date <= s.EndDate.Date);
                if (activeSeason != null)
                {
                    nightPrice *= activeSeason.PriceMultiplier;
                }
                totalPrice += nightPrice;
            }

            // Update booking details
            booking.UnitId = unitId;
            booking.CheckInDate = checkIn;
            booking.CheckOutDate = checkOut;
            booking.TotalPrice = totalPrice;

            await _context.SaveChangesAsync(default);

            return Json(new { success = true, message = "Reservation rescheduled successfully.", totalPrice = totalPrice });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error updating reservation: {ex.Message}" });
        }
    }
}
