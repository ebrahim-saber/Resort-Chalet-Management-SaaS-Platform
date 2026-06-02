using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Features.Analytics.Queries.GetDashboardStats;

using Microsoft.AspNetCore.Authorization;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Authorize]
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
        DashboardStatsDto? stats = null;
        string resortName = "Luxury Management Portal";

        try
        {
            var resort = await _context.Resorts.FirstOrDefaultAsync();
            if (resort != null)
            {
                var query = new GetDashboardStatsQuery(resort.Id, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1));
                stats = await _mediator.Send(query);
                resortName = resort.Name;
            }
        }
        catch (Exception)
        {
            // Database is offline/unreachable
        }

        if (stats == null)
        {
            stats = new DashboardStatsDto(
                TotalRevenue: 0.00m,
                PendingRevenue: 0.00m,
                OccupancyRate: 0.00,
                TotalReservations: 0,
                ActiveMaintenanceRequests: 0,
                DirtyUnitsCount: 0
            );
        }

        ViewBag.ResortName = resortName;

        // Fetch recent reservations for dashboard table
        List<ResortManagement.Domain.Entities.Reservations.Reservation> recentReservations = new();
        List<ResortManagement.Domain.Entities.Resorts.Resort> topResorts = new();
        try
        {
            recentReservations = await _context.Reservations
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            topResorts = await _context.Resorts
                .Take(2)
                .ToListAsync();
        }
        catch (Exception)
        {
            // Database is unreachable
        }

        ViewBag.RecentReservations = recentReservations;
        ViewBag.TopResorts = topResorts;

        return View(stats);
    }
}
