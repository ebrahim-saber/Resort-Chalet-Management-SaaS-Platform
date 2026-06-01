using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Resorts;

namespace ResortManagement.Application.Features.Resorts.Commands.CreateResort;

public record CreateResortCommand(
    string Name,
    string Address,
    string? ContactNumber
) : IRequest<Guid>;

public class CreateResortCommandHandler : IRequestHandler<CreateResortCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CreateResortCommandHandler(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateResortCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        var resort = new Resort(tenantId, request.Name, request.Address, request.ContactNumber);

        _context.Resorts.Add(resort);
        await _context.SaveChangesAsync(cancellationToken);

        return resort.Id;
    }
}
