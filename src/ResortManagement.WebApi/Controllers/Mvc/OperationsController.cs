using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Operations;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Route("operations")]
public class OperationsController : Controller
{
    private readonly IApplicationDbContext _context;

    public OperationsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("housekeeping")]
    public async Task<IActionResult> Housekeeping()
    {
        List<HousekeepingTask> tasks = new();
        try
        {
            tasks = await _context.HousekeepingTasks.ToListAsync();
        }
        catch (Exception)
        {
            // Database is offline/unreachable, fallback to simulated tasks below
        }

        if (tasks.Count == 0)
        {
            ViewBag.ShowcaseTasks = new List<dynamic>
            {
                new { Id = Guid.NewGuid(), UnitNumber = "Suite 302", Status = "Pending", Notes = "Checkout cleaning. Change linens, sanitize kitchen.", CreatedAt = DateTime.UtcNow.AddHours(-2) },
                new { Id = Guid.NewGuid(), UnitNumber = "Villa 105", Status = "InProgress", Notes = "Daily routine cleanup and towels refresh.", CreatedAt = DateTime.UtcNow.AddHours(-1) },
                new { Id = Guid.NewGuid(), UnitNumber = "Chalet 204", Status = "Completed", Notes = "Deep clean jacuzzi and pool terrace completed.", CreatedAt = DateTime.UtcNow.AddHours(-4) }
            };
        }
        else
        {
            ViewBag.ShowcaseTasks = tasks;
        }

        return View();
    }

    [HttpGet("maintenance")]
    public async Task<IActionResult> Maintenance()
    {
        List<MaintenanceRequest> requests = new();
        try
        {
            requests = await _context.MaintenanceRequests.ToListAsync();
        }
        catch (Exception)
        {
            // Database is offline/unreachable, fallback to simulated requests below
        }

        if (requests.Count == 0)
        {
            ViewBag.ShowcaseRequests = new List<dynamic>
            {
                new { Id = Guid.NewGuid(), UnitNumber = "Chalet 104", Title = "AC Compressor Failure", Priority = "Critical", Status = "InProgress", CreatedAt = DateTime.UtcNow.AddHours(-3), Description = "AC unit is blowing hot air on Friday weekend Rush." },
                new { Id = Guid.NewGuid(), UnitNumber = "Suite 201", Title = "Jacuzzi Heater Leak", Priority = "High", Status = "Open", CreatedAt = DateTime.UtcNow.AddHours(-5), Description = "Minor water leak underneath pool deck." },
                new { Id = Guid.NewGuid(), UnitNumber = "Villa 202", Title = "Terrace Light Replacement", Priority = "Medium", Status = "Resolved", CreatedAt = DateTime.UtcNow.AddDays(-1), Description = "Three light bulbs burnt out." }
            };
        }
        else
        {
            ViewBag.ShowcaseRequests = requests;
        }

        return View();
    }
}
