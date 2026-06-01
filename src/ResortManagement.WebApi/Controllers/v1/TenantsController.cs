using System;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Models;
using ResortManagement.Application.Features.SaaS.Commands.RegisterTenant;

namespace ResortManagement.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tenants")]
[ApiController]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<Guid>>> Register([FromBody] RegisterTenantCommand command)
    {
        var tenantId = await _mediator.Send(command);
        return Ok(new ApiResponse<Guid>(tenantId, "Tenant workspace initialized and registered successfully."));
    }
}
