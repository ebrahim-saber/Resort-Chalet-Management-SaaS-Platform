using System;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Models;
using ResortManagement.Application.Features.Billing.Commands.ProcessPayment;

namespace ResortManagement.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/billing")]
[ApiController]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IMediator _mediator;

    public BillingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("payments")]
    public async Task<ActionResult<ApiResponse<Guid>>> ProcessPayment([FromBody] ProcessPaymentCommand command)
    {
        var paymentId = await _mediator.Send(command);
        return Ok(new ApiResponse<Guid>(paymentId, "Payment processed successfully."));
    }
}
