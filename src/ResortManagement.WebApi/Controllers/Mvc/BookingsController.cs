using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Resorts;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Route("bookings")]
public class BookingsController : Controller
{
    private readonly IApplicationDbContext _context;

    public BookingsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var unitTypes = await _context.UnitTypes.ToListAsync();

        if (unitTypes.Count == 0)
        {
            // Seed a list of luxury room types if none exist yet for demonstration
            ViewBag.ShowcaseUnitTypes = new List<dynamic>
            {
                new { Id = Guid.NewGuid(), Name = "Royal Pool Chalet", BasePrice = 450.00m, MaxOccupancy = 4, Amenities = "Private Infinity Pool, Sea View, Butler Service" },
                new { Id = Guid.NewGuid(), Name = "Premium Sea Villa", BasePrice = 320.00m, MaxOccupancy = 6, Amenities = "Beach Access, Kitchenette, Spacious Terrace" },
                new { Id = Guid.NewGuid(), Name = "Boutique Suite", BasePrice = 180.00m, MaxOccupancy = 2, Amenities = "King Bed, Frosted Jacuzzi, High-speed Wifi" }
            };
        }
        else
        {
            ViewBag.ShowcaseUnitTypes = unitTypes;
        }

        return View();
    }
}
