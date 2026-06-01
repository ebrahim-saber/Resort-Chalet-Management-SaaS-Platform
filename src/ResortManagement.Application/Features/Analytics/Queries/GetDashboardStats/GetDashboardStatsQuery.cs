using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;

namespace ResortManagement.Application.Features.Analytics.Queries.GetDashboardStats;

public record GetDashboardStatsQuery(Guid ResortId, DateTime StartDate, DateTime EndDate) : IRequest<DashboardStatsDto>;

public record DashboardStatsDto(
    decimal TotalRevenue,
    decimal PendingRevenue,
    double OccupancyRate,
    int TotalReservations,
    int ActiveMaintenanceRequests,
    int DirtyUnitsCount
);

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // 1. Total Revenue (Successful payments)
        var totalRevenue = await _context.Payments
            .Where(p => p.Status == "Succeeded" && p.PaidAt >= request.StartDate && p.PaidAt <= request.EndDate)
            .SumAsync(p => p.Amount, cancellationToken);

        // 2. Pending Revenue (Unpaid invoices due in range)
        var pendingRevenue = await _context.Invoices
            .Where(i => (i.Status == "Unpaid" || i.Status == "PartiallyPaid") && i.DueDate >= request.StartDate && i.DueDate <= request.EndDate)
            .SumAsync(i => i.TotalAmount, cancellationToken);

        // 3. Occupancy calculation
        // Filter units belonging to the requested resort
        var resortUnitsQuery = _context.Units
            .Where(u => _context.Floors.Any(f => f.Id == u.FloorId && _context.Buildings.Any(b => b.Id == f.BuildingId && b.ResortId == request.ResortId)));

        var totalUnitsCount = await resortUnitsQuery.CountAsync(cancellationToken);

        // Total occupied units (reservations active during the date range)
        var occupiedUnitsCount = await _context.Reservations
            .Where(r => r.Status != "Cancelled" && r.CheckInDate < request.EndDate && r.CheckOutDate > request.StartDate)
            .Select(r => r.UnitId)
            .Distinct()
            .CountAsync(cancellationToken);

        double occupancyRate = totalUnitsCount > 0 ? ((double)occupiedUnitsCount / totalUnitsCount) * 100 : 0;

        // 4. Total reservations count
        var totalReservations = await _context.Reservations
            .CountAsync(r => r.CheckInDate >= request.StartDate && r.CheckInDate <= request.EndDate && r.Status != "Cancelled", cancellationToken);

        // 5. Active maintenance requests
        var activeMaintenanceRequests = await _context.MaintenanceRequests
            .CountAsync(m => m.Status != "Resolved" && m.Status != "Closed", cancellationToken);

        // 6. Dirty units count
        var dirtyUnitsCount = await resortUnitsQuery
            .CountAsync(u => u.HousekeepingStatus == "Dirty", cancellationToken);

        return new DashboardStatsDto(
            totalRevenue,
            pendingRevenue,
            Math.Round(occupancyRate, 2),
            totalReservations,
            activeMaintenanceRequests,
            dirtyUnitsCount
        );
    }
}
