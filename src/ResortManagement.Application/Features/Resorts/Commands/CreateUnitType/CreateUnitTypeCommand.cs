using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Domain.Entities.Resorts;

namespace ResortManagement.Application.Features.Resorts.Commands.CreateUnitType;

public record CreateUnitTypeCommand(
    Guid ResortId,
    string Name,
    decimal BasePrice,
    int MaxOccupancy
) : IRequest<Guid>;

public class CreateUnitTypeCommandHandler : IRequestHandler<CreateUnitTypeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CreateUnitTypeCommandHandler(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateUnitTypeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;

        // Verify resort exists (filtered automatically by tenant query filter!)
        var resortExists = await _context.Resorts.AnyAsync(r => r.Id == request.ResortId, cancellationToken);
        if (!resortExists)
        {
            throw new NotFoundException(nameof(Resort), request.ResortId);
        }

        var unitType = new UnitType(tenantId, request.ResortId, request.Name, request.BasePrice, request.MaxOccupancy);
        _context.UnitTypes.Add(unitType);
        await _context.SaveChangesAsync(cancellationToken);

        return unitType.Id;
    }
}
