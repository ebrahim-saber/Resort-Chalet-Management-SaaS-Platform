using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.CRM;
using ResortManagement.Domain.Entities.Reservations;
using ResortManagement.Domain.Entities.Resorts;
using ResortManagement.Domain.Entities.Billing;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Route("storefront")]
public class StorefrontController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public StorefrontController(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public class StorefrontChaletDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ResortName { get; set; } = null!;
        public string BuildingName { get; set; } = null!;
        public int MaxCapacity { get; set; }
        public decimal BasePrice { get; set; }
        public Guid UnitTypeId { get; set; }
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(DateTime? checkIn, DateTime? checkOut, int? guestsCount)
    {
        // 1. Resolve first Tenant ID to bind the guest public portal context dynamically
        var tenant = await _context.Tenants.FirstOrDefaultAsync();
        if (tenant == null)
        {
            return Content("No active tenants found.");
        }
        
        // Expose tenant context dynamically
        _tenantProvider.SetTenantId(tenant.Id);

        var start = checkIn ?? DateTime.Today.AddDays(1);
        var end = checkOut ?? start.AddDays(3);
        var guests = guestsCount ?? 2;

        // 2. Fetch available units by joining Floors -> Buildings -> Resorts
        var allUnits = await (from u in _context.Units
                              join ut in _context.UnitTypes on u.UnitTypeId equals ut.Id
                              join f in _context.Floors on u.FloorId equals f.Id
                              join b in _context.Buildings on f.BuildingId equals b.Id
                              join r in _context.Resorts on b.ResortId equals r.Id
                              where u.IsActive && !u.IsDeleted && ut.MaxOccupancy >= guests
                              select new StorefrontChaletDto
                              {
                                  Id = u.Id,
                                  Name = u.UnitNumber,
                                  ResortName = r.Name,
                                  BuildingName = b.Name,
                                  MaxCapacity = ut.MaxOccupancy,
                                  BasePrice = ut.BasePrice,
                                  UnitTypeId = ut.Id
                              }).ToListAsync();

        var overlappingReservationUnitIds = await _context.Reservations
            .Where(r => r.Status != "Cancelled" && r.CheckInDate < end && r.CheckOutDate > start && !r.IsDeleted)
            .Select(r => r.UnitId)
            .Distinct()
            .ToListAsync();

        var availableUnits = allUnits
            .Where(u => !overlappingReservationUnitIds.Contains(u.Id))
            .ToList();

        // 3. Compute dynamic pricing preview for each available unit
        var seasons = await _context.Seasons
            .Where(s => start < s.EndDate && end > s.StartDate && !s.IsDeleted)
            .ToListAsync();

        var pricingRules = await _context.PricingRules
            .Where(pr => (pr.RuleType == "Weekend" || (pr.SpecificDate >= start && pr.SpecificDate < end)) && !pr.IsDeleted)
            .ToListAsync();

        var allSeasons = await _context.Seasons.Where(s => !s.IsDeleted).ToListAsync();
        var allRules = await _context.PricingRules.Where(r => !r.IsDeleted).ToListAsync();

        var unitPricingPreviews = new Dictionary<Guid, decimal>();
        foreach (var unit in availableUnits)
        {
            decimal totalPrice = 0;
            for (var date = start.Date; date < end.Date; date = date.AddDays(1))
            {
                decimal nightPrice = unit.BasePrice;
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

                totalPrice += nightPrice;
            }
            unitPricingPreviews[unit.Id] = totalPrice;
        }

        var resorts = await _context.Resorts.Where(r => !r.IsDeleted).ToListAsync();

        ViewBag.CheckIn = start;
        ViewBag.CheckOut = end;
        ViewBag.Guests = guests;
        ViewBag.AvailableUnits = availableUnits;
        ViewBag.PricingPreviews = unitPricingPreviews;
        ViewBag.Tenant = tenant;
        ViewBag.SeasonsList = allSeasons;
        ViewBag.PricingRulesList = allRules;
        ViewBag.Resorts = resorts;

        return View();
    }

    [HttpPost("book")]
    public async Task<IActionResult> BookStay(
        Guid unitId, 
        DateTime checkIn, 
        DateTime checkOut, 
        string firstName, 
        string lastName, 
        string email, 
        string phone,
        string documentNumber)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync();
        if (tenant == null)
        {
            return RedirectToAction("Index");
        }

        _tenantProvider.SetTenantId(tenant.Id);

        try
        {
            // 1. Verify Unit
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);

            if (unit == null || !unit.IsActive)
            {
                TempData["Error"] = "Selected unit is not active or unavailable.";
                return RedirectToAction("Index");
            }

            var unitType = await _context.UnitTypes
                .FirstOrDefaultAsync(ut => ut.Id == unit.UnitTypeId && !ut.IsDeleted);

            if (unitType == null)
            {
                TempData["Error"] = "Unit type details not found.";
                return RedirectToAction("Index");
            }

            // 2. Overlap Verification
            var isOccupied = await _context.Reservations
                .AnyAsync(r => r.UnitId == unitId && r.Status != "Cancelled" && r.CheckInDate < checkOut && r.CheckOutDate > checkIn && !r.IsDeleted);

            if (isOccupied)
            {
                TempData["Error"] = "The unit is already double-booked for those dates. Please search again.";
                return RedirectToAction("Index");
            }

            // 3. Find or register Customer (CRM Guest)
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted);

            if (customer == null)
            {
                customer = new Customer(tenant.Id, firstName, lastName, email, phone, documentNumber, "Guest");
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync(default);
            }

            // 4. Compute Dynamic Season/Weekend Pricing
            var seasons = await _context.Seasons
                .Where(s => checkIn < s.EndDate && checkOut > s.StartDate && !s.IsDeleted)
                .ToListAsync();

            var pricingRules = await _context.PricingRules
                .Where(pr => (pr.RuleType == "Weekend" || (pr.SpecificDate >= checkIn && pr.SpecificDate < checkOut)) && !pr.IsDeleted)
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

                if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
                {
                    var weekendRule = pricingRules.FirstOrDefault(pr => pr.RuleType == "Weekend");
                    if (weekendRule != null)
                    {
                        if (weekendRule.Multiplier.HasValue)
                        {
                            nightPrice *= weekendRule.Multiplier.Value;
                        }
                    }
                }

                totalPrice += nightPrice;
            }

            // 5. Create reservation
            var reservation = new Reservation(tenant.Id, customer.Id, unitId, checkIn, checkOut, totalPrice);
            reservation.Confirm(); // Guests booking automatically triggers Confirmed stays in SaaS model!
            _context.Reservations.Add(reservation);

            // 6. Generate Invoice
            var invoice = new Invoice(tenant.Id, reservation.Id, "INV-" + DateTime.UtcNow.Ticks.ToString().Substring(10), totalPrice, DateTime.UtcNow.AddDays(1));
            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync(default);

            TempData["Success"] = $"Booking completed successfully. Reservation ID: {reservation.Id.ToString().Substring(0,8)}. Total: {totalPrice.ToString("C")}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Booking failed: {ex.Message}";
        }

        return RedirectToAction("Index");
    }
}
