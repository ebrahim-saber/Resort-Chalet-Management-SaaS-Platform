using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.CRM;

namespace ResortManagement.Application.Features.CRM.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string? Email,
    string Phone,
    string? IdentityNumber,
    string? Nationality
) : IRequest<Guid>;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CreateCustomerCommandHandler(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        var customer = new Customer(
            tenantId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.IdentityNumber,
            request.Nationality);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
