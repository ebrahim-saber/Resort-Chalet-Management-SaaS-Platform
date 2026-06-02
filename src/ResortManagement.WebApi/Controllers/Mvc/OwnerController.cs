using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Authorize(Roles = "Administrator,Operations Manager")]
[Route("owner")]
public class OwnerController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public OwnerController(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var tenantId = _tenantProvider.TenantId;

        // 1. Fetch properties owned under tenant
        var resorts = await _context.Resorts.ToListAsync();
        var resortCount = resorts.Count;

        // 2. Fetch reservation counts and revenue stats from database
        var totalReservations = await _context.Reservations.CountAsync(r => r.Status != "Cancelled");
        var grossRevenue = await _context.Payments
            .Where(p => p.Status == "Succeeded")
            .SumAsync(p => p.Amount);

        // 3. Expected Occupancy forecasting (based on bookings in Q3/Q4 2026)
        var Q3StartDate = new DateTime(2026, 7, 1);
        var Q3EndDate = new DateTime(2026, 9, 30);
        var Q3BookingsCount = await _context.Reservations
            .CountAsync(r => r.Status != "Cancelled" && r.CheckInDate < Q3EndDate && r.CheckOutDate > Q3StartDate);

        var totalUnitsCount = await _context.Units.CountAsync();
        double Q3ForecastOccupancy = totalUnitsCount > 0 ? Math.Round(((double)Q3BookingsCount / totalUnitsCount) * 100, 1) : 75.0;

        // 4. Property Expenditure (Maintenance expenses + seeder cleanup)
        var maintenanceExpenses = await _context.MaintenanceRequests
            .Where(m => m.Status == "Resolved" || m.Status == "Closed")
            .CountAsync() * 150.00m; // Flat estimated cost per resolved ticket
        
        var operationalHousekeepingCosts = await _context.HousekeepingTasks
            .CountAsync() * 45.00m; // Housekeeping payroll cost

        var totalExpenses = maintenanceExpenses + operationalHousekeepingCosts;
        var netRevenue = grossRevenue - totalExpenses;

        // Pass variables to view
        ViewBag.ResortsCount = resortCount;
        ViewBag.TotalReservations = totalReservations;
        ViewBag.GrossRevenue = grossRevenue;
        ViewBag.NetRevenue = netRevenue;
        ViewBag.TotalExpenses = totalExpenses;
        ViewBag.Q3Forecast = Q3ForecastOccupancy;
        ViewBag.Resorts = resorts;

        return View();
    }
}
