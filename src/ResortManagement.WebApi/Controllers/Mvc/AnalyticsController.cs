using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Features.Analytics.Queries.GetDashboardStats;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Authorize(Roles = "Administrator,Operations Manager")]
[Route("analytics")]
public class AnalyticsController : Controller
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public AnalyticsController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var resort = await _context.Resorts.FirstOrDefaultAsync();
        Guid resortId = resort?.Id ?? Guid.Empty;

        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(30);

        DashboardStatsDto stats = null;
        try
        {
            var query = new GetDashboardStatsQuery(resortId, startDate, endDate);
            stats = await _mediator.Send(query);
        }
        catch (Exception)
        {
            // Database unreachable or query error, fallback to simulated stats
        }

        if (stats == null)
        {
            ViewBag.Stats = new
            {
                TotalRevenue = 0.00m,
                PendingRevenue = 0.00m,
                OccupancyRate = 0.00,
                TotalReservations = 0,
                ActiveMaintenanceRequests = 0,
                DirtyUnitsCount = 0
            };
        }
        else
        {
            ViewBag.Stats = new
            {
                TotalRevenue = stats.TotalRevenue,
                PendingRevenue = stats.PendingRevenue,
                OccupancyRate = stats.OccupancyRate,
                TotalReservations = stats.TotalReservations,
                ActiveMaintenanceRequests = stats.ActiveMaintenanceRequests,
                DirtyUnitsCount = stats.DirtyUnitsCount
            };
        }

        return View();
    }
}
