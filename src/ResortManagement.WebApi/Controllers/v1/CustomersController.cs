using System;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Models;
using ResortManagement.Application.Features.CRM.Commands.CreateCustomer;

namespace ResortManagement.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customers")]
[ApiController]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateCustomerCommand command)
    {
        var customerId = await _mediator.Send(command);
        return Ok(new ApiResponse<Guid>(customerId, "Customer profile created successfully."));
    }
}
