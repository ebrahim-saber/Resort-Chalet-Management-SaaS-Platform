using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Resorts;

using Microsoft.AspNetCore.Authorization;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Authorize]
[Route("resorts")]
public class ResortsController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ResortsController(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        List<dynamic> resorts = new();
        int overallActiveUnits = 0;
        int overallOccupancy = 0;
        
        int chaletsCount = 0;
        int villasCount = 0;
        int suitesCount = 0;
        
        int chaletsPct = 0;
        int villasPct = 0;
        int suitesPct = 0;

        try
        {
            var dbResorts = await _context.Resorts.ToListAsync();
            var dbUnits = await _context.Units.ToListAsync();
            var dbUnitTypes = await _context.UnitTypes.ToDictionaryAsync(ut => ut.Id);

            overallActiveUnits = dbUnits.Count(u => u.IsActive);
            
            var occupiedCount = await _context.Reservations
                .CountAsync(res => res.Status != "Cancelled" && res.CheckInDate <= DateTime.UtcNow && res.CheckOutDate >= DateTime.UtcNow);

            overallOccupancy = overallActiveUnits > 0 ? (int)Math.Round(((double)occupiedCount / overallActiveUnits) * 100, 0) : 0;

            foreach (var u in dbUnits)
            {
                if (dbUnitTypes.TryGetValue(u.UnitTypeId, out var ut))
                {
                    if (ut.Name.Contains("Chalet", StringComparison.OrdinalIgnoreCase)) chaletsCount++;
                    else if (ut.Name.Contains("Villa", StringComparison.OrdinalIgnoreCase)) villasCount++;
                    else if (ut.Name.Contains("Suite", StringComparison.OrdinalIgnoreCase)) suitesCount++;
                }
            }

            int totalCategorized = chaletsCount + villasCount + suitesCount;
            if (totalCategorized > 0)
            {
                chaletsPct = (int)Math.Round(((double)chaletsCount / totalCategorized) * 100, 0);
                villasPct = (int)Math.Round(((double)villasCount / totalCategorized) * 100, 0);
                suitesPct = (int)Math.Round(((double)suitesCount / totalCategorized) * 100, 0);
            }

            foreach (var r in dbResorts)
            {
                var buildings = await _context.Buildings.Where(b => b.ResortId == r.Id).Select(b => b.Id).ToListAsync();
                var floors = await _context.Floors.Where(f => buildings.Contains(f.BuildingId)).Select(f => f.Id).ToListAsync();
                var units = dbUnits.Where(u => floors.Contains(u.FloorId)).ToList();
                
                int totalUnits = units.Count;
                
                var occupiedResortUnits = await _context.Reservations
                    .Where(res => res.Status != "Cancelled" && res.CheckInDate <= DateTime.UtcNow && res.CheckOutDate >= DateTime.UtcNow && units.Select(u => u.Id).Contains(res.UnitId))
                    .CountAsync();

                double occupancyRate = totalUnits > 0 ? Math.Round(((double)occupiedResortUnits / totalUnits) * 100, 0) : 0;

                decimal resortRevenue = 0;
                try
                {
                    var resortUnitIds = units.Select(u => u.Id).ToList();
                    var resortReservations = await _context.Reservations
                        .Where(res => resortUnitIds.Contains(res.UnitId))
                        .Select(res => res.Id)
                        .ToListAsync();

                    resortRevenue = await _context.Invoices
                        .Where(inv => resortReservations.Contains(inv.ReservationId) && inv.Status == "Paid")
                        .SumAsync(inv => inv.TotalAmount);
                }
                catch (Exception) { }

                decimal resortRevPAR = totalUnits > 0 ? resortRevenue / totalUnits : 0;

                resorts.Add(new { 
                    Id = r.Id, 
                    Name = r.Name,
                    Address = r.Address,
                    ContactNumber = r.ContactNumber,
                    TotalUnits = totalUnits,
                    Occupancy = occupancyRate,
                    RevPAR = resortRevPAR
                });
            }
        }
        catch (Exception)
        {
            // Database offline, fallbacks handle gracefully
        }

        ViewBag.Resorts = resorts;
        ViewBag.OverallActiveUnits = overallActiveUnits;
        ViewBag.OverallOccupancy = overallOccupancy;
        ViewBag.LuxuryChaletsCount = chaletsCount;
        ViewBag.LuxuryChaletsPct = chaletsPct;
        ViewBag.VillasCount = villasCount;
        ViewBag.VillasPct = villasPct;
        ViewBag.SuitesCount = suitesCount;
        ViewBag.SuitesPct = suitesPct;

        return View();
    }

    [HttpGet("units")]
    public async Task<IActionResult> Units()
    {
        List<dynamic> units = new();
        int totalUnits = 0;
        int occupiedCount = 0;
        List<string> locations = new();
        try
        {
            var dbUnits = await _context.Units.ToListAsync();
            var dbUnitTypes = await _context.UnitTypes.ToDictionaryAsync(ut => ut.Id);
            var dbFloors = await _context.Floors.ToDictionaryAsync(f => f.Id);
            var dbBuildings = await _context.Buildings.ToDictionaryAsync(b => b.Id);
            var dbResorts = await _context.Resorts.ToDictionaryAsync(r => r.Id);
            
            totalUnits = dbUnits.Count;
            occupiedCount = await _context.Reservations
                .CountAsync(res => res.Status != "Cancelled" && res.CheckInDate <= DateTime.UtcNow && res.CheckOutDate >= DateTime.UtcNow);

            locations = dbResorts.Values.Select(r => r.Name).Distinct().ToList();

            foreach (var u in dbUnits)
            {
                dbUnitTypes.TryGetValue(u.UnitTypeId, out var ut);
                
                dbFloors.TryGetValue(u.FloorId, out var floor);
                var building = floor != null ? dbBuildings.GetValueOrDefault(floor.BuildingId) : null;
                var resort = building != null ? dbResorts.GetValueOrDefault(building.ResortId) : null;

                var isOccupied = await _context.Reservations.AnyAsync(res => res.UnitId == u.Id && res.Status != "Cancelled" && res.CheckInDate <= DateTime.UtcNow && res.CheckOutDate >= DateTime.UtcNow);
                string status = isOccupied ? "Occupied" : (u.HousekeepingStatus == "OutOfService" ? "Maintenance" : "Available");

                units.Add(new { 
                    Id = u.Id, 
                    Number = u.UnitNumber, 
                    Status = status, 
                    UnitTypeName = ut?.Name ?? "Standard Room",
                    BasePrice = ut?.BasePrice ?? 500,
                    MaxOccupancy = ut?.MaxOccupancy ?? 2,
                    ResortName = resort?.Name ?? "Alpine Peak Resort",
                    ResortAddress = resort?.Address ?? "Zermatt, Swiss Alps"
                });
            }
        }
        catch (Exception)
        {
            // Database offline
        }

        ViewBag.Units = units;
        ViewBag.TotalUnits = totalUnits;
        ViewBag.Occupancy = totalUnits > 0 ? (int)Math.Round(((double)occupiedCount / totalUnits) * 100) : 0;
        ViewBag.Locations = locations;
        return View();
    }

    [Authorize(Roles = "Administrator,Operations Manager")]
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Administrator,Operations Manager")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(string name, string address, string contactNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "Resort Name is required.");
            return View();
        }

        var tenantId = _tenantProvider.TenantId;
        var resort = new Resort(tenantId, name, address, contactNumber);
        
        _context.Resorts.Add(resort);
        await _context.SaveChangesAsync(default);

        var building = new Building(tenantId, resort.Id, "Main Lodge");
        _context.Buildings.Add(building);
        await _context.SaveChangesAsync(default);

        var floor = new Floor(tenantId, building.Id, 1);
        _context.Floors.Add(floor);
        await _context.SaveChangesAsync(default);

        try
        {
            var newNotif = new ResortManagement.Domain.Entities.Operations.Notification(
                tenantId,
                null,
                "System",
                "admin",
                "New Resort Registered",
                $"Resort '{name}' has been successfully provisioned into the portfolio."
            );
            _context.Notifications.Add(newNotif);
            await _context.SaveChangesAsync(default);
        }
        catch (Exception) { }

        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Administrator,Operations Manager")]
    [HttpGet("unit-types/create")]
    public async Task<IActionResult> CreateUnitType()
    {
        ViewBag.Resorts = await _context.Resorts.ToListAsync();
        return View();
    }

    [Authorize(Roles = "Administrator,Operations Manager")]
    [HttpPost("unit-types/create")]
    public async Task<IActionResult> CreateUnitType(Guid resortId, string name, decimal basePrice, int maxOccupancy)
    {
        if (string.IsNullOrWhiteSpace(name) || resortId == Guid.Empty)
        {
            ModelState.AddModelError("", "Resort and Unit Type Name are required.");
            ViewBag.Resorts = await _context.Resorts.ToListAsync();
            return View();
        }

        var tenantId = _tenantProvider.TenantId;
        var unitType = new UnitType(tenantId, resortId, name, basePrice, maxOccupancy);

        _context.UnitTypes.Add(unitType);
        await _context.SaveChangesAsync(default);

        return RedirectToAction("Units");
    }

    [Authorize(Roles = "Administrator,Operations Manager")]
    [HttpGet("units/create")]
    public async Task<IActionResult> CreateUnit()
    {
        ViewBag.Resorts = await _context.Resorts.ToListAsync();
        ViewBag.UnitTypes = await _context.UnitTypes.ToListAsync();
        return View();
    }

    [Authorize(Roles = "Administrator,Operations Manager")]
    [HttpPost("units/create")]
    public async Task<IActionResult> CreateUnit(Guid resortId, Guid unitTypeId, string unitNumber)
    {
        if (string.IsNullOrWhiteSpace(unitNumber) || resortId == Guid.Empty || unitTypeId == Guid.Empty)
        {
            ModelState.AddModelError("", "All fields are required.");
            ViewBag.Resorts = await _context.Resorts.ToListAsync();
            ViewBag.UnitTypes = await _context.UnitTypes.ToListAsync();
            return View();
        }

        var tenantId = _tenantProvider.TenantId;

        var building = await _context.Buildings.FirstOrDefaultAsync(b => b.ResortId == resortId);
        if (building == null)
        {
            building = new Building(tenantId, resortId, "Main Lodge");
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync(default);
        }

        var floor = await _context.Floors.FirstOrDefaultAsync(f => f.BuildingId == building.Id);
        if (floor == null)
        {
            floor = new Floor(tenantId, building.Id, 1);
            _context.Floors.Add(floor);
            await _context.SaveChangesAsync(default);
        }

        var unit = new Unit(tenantId, floor.Id, unitTypeId, unitNumber);
        _context.Units.Add(unit);
        await _context.SaveChangesAsync(default);

        try
        {
            var newNotif = new ResortManagement.Domain.Entities.Operations.Notification(
                tenantId,
                null,
                "System",
                "admin",
                "New Room Unit Registered",
                $"Room unit '{unitNumber}' successfully registered under Resort '{building.Name}' layout."
            );
            _context.Notifications.Add(newNotif);
            await _context.SaveChangesAsync(default);
        }
        catch (Exception) { }

        return RedirectToAction("Units");
    }
}
