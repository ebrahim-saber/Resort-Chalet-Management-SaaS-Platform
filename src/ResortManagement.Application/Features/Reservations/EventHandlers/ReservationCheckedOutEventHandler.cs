using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Operations;
using ResortManagement.Domain.Entities.Reservations.Events;

namespace ResortManagement.Application.Features.Reservations.EventHandlers;

public class ReservationCheckedOutEventHandler : INotificationHandler<ReservationCheckedOutEvent>
{
    private readonly IApplicationDbContext _context;

    public ReservationCheckedOutEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReservationCheckedOutEvent notification, CancellationToken cancellationToken)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == notification.UnitId, cancellationToken);

        if (unit != null)
        {
            unit.MarkDirty();

            var task = new HousekeepingTask(
                notification.TenantId,
                notification.UnitId,
                null,
                $"Automated task: Unit marked dirty due to checkout of reservation {notification.ReservationId}."
            );

            _context.HousekeepingTasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
