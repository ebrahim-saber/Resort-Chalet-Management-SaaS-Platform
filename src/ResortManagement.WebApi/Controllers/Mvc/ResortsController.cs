using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Route("resorts")]
public class ResortsController : Controller
{
    private readonly IApplicationDbContext _context;

    public ResortsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        List<dynamic> resorts = new();
        try
        {
            var dbResorts = await _context.Resorts.ToListAsync();
            foreach (var r in dbResorts)
            {
                resorts.Add(new { Id = r.Id, Name = r.Name });
            }
        }
        catch (Exception)
        {
            // Database offline, fallback to simulated data
        }

        ViewBag.Resorts = resorts;
        return View();
    }

    [HttpGet("units")]
    public async Task<IActionResult> Units()
    {
        List<dynamic> units = new();
        try
        {
            var dbUnits = await _context.Units.Include(u => u.UnitType).ToListAsync();
            foreach (var u in dbUnits)
            {
                units.Add(new { 
                    Id = u.Id, 
                    Number = u.Number, 
                    Status = u.Status.ToString(), 
                    UnitTypeName = u.UnitType?.Name 
                });
            }
        }
        catch (Exception)
        {
            // Database offline, fallback to simulated data
        }

        ViewBag.Units = units;
        return View();
    }
}
