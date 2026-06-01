using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Domain.Entities.Operations;
using UnitEntity = ResortManagement.Domain.Entities.Resorts.Unit;

namespace ResortManagement.Application.Features.Operations.Commands.CreateMaintenanceRequest;

public record CreateMaintenanceRequestCommand(
    Guid UnitId,
    Guid RequestedById,
    string Title,
    string Description,
    string Priority
) : IRequest<Guid>;

public class CreateMaintenanceRequestCommandHandler : IRequestHandler<CreateMaintenanceRequestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CreateMaintenanceRequestCommandHandler(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == request.UnitId, cancellationToken);
        if (unit == null)
        {
            throw new NotFoundException(nameof(UnitEntity), request.UnitId);
        }

        var maintenanceRequest = new MaintenanceRequest(
            tenantId,
            request.UnitId,
            request.RequestedById,
            request.Title,
            request.Description,
            request.Priority
        );

        // Block unit for maintenance if priority is critical or high
        if (request.Priority == "Critical" || request.Priority == "High")
        {
            unit.BlockForMaintenance();
        }

        _context.MaintenanceRequests.Add(maintenanceRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return maintenanceRequest.Id;
    }
}
