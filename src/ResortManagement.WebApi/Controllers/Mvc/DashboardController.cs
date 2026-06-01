using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Features.Analytics.Queries.GetDashboardStats;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Route("dashboard")]
public class DashboardController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public DashboardController(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        // 1. Try to get first Resort in DB
        var resort = await _context.Resorts.FirstOrDefaultAsync();
        
        DashboardStatsDto stats;
        
        if (resort != null)
        {
            var query = new GetDashboardStatsQuery(resort.Id, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1));
            stats = await _mediator.Send(query);
            ViewBag.ResortName = resort.Name;
        }
        else
        {
            // Seeding fallback simulated luxury statistics to WOW the user immediately
            stats = new DashboardStatsDto(
                TotalRevenue: 148250.00m,
                PendingRevenue: 12400.00m,
                OccupancyRate: 78.45,
                TotalReservations: 142,
                ActiveMaintenanceRequests: 3,
                DirtyUnitsCount: 5
            );
            ViewBag.ResortName = "LuxeStay Royal Resort (Simulated)";
        }

        // Fetch recent reservations for dashboard table
        var recentReservations = await _context.Reservations
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.RecentReservations = recentReservations;

        return View(stats);
    }
}
