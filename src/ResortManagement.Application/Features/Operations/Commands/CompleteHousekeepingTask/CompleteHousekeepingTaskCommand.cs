using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Domain.Entities.Operations;

namespace ResortManagement.Application.Features.Operations.Commands.CompleteHousekeepingTask;

public record CompleteHousekeepingTaskCommand(Guid TaskId) : IRequest<bool>;

public class CompleteHousekeepingTaskCommandHandler : IRequestHandler<CompleteHousekeepingTaskCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CompleteHousekeepingTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CompleteHousekeepingTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.HousekeepingTasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException(nameof(HousekeepingTask), request.TaskId);
        }

        task.CompleteTask();

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == task.UnitId, cancellationToken);

        if (unit != null)
        {
            unit.MarkClean();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
