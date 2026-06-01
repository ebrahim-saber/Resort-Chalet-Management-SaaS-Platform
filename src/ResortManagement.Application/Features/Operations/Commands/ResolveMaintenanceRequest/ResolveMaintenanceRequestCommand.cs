using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Domain.Entities.Operations;

namespace ResortManagement.Application.Features.Operations.Commands.ResolveMaintenanceRequest;

public record ResolveMaintenanceRequestCommand(Guid RequestId) : IRequest<bool>;

public class ResolveMaintenanceRequestCommandHandler : IRequestHandler<ResolveMaintenanceRequestCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ResolveMaintenanceRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ResolveMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _context.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

        if (maintenanceRequest == null)
        {
            throw new NotFoundException(nameof(MaintenanceRequest), request.RequestId);
        }

        maintenanceRequest.Resolve();

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == maintenanceRequest.UnitId, cancellationToken);

        if (unit != null)
        {
            // After resolving maintenance, mark the unit dirty so housekeeping handles cleanup
            unit.MarkDirty();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
