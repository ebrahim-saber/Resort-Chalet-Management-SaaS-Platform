using System;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Models;
using ResortManagement.Application.Features.Operations.Commands.CompleteHousekeepingTask;
using ResortManagement.Application.Features.Operations.Commands.CreateMaintenanceRequest;
using ResortManagement.Application.Features.Operations.Commands.ResolveMaintenanceRequest;

namespace ResortManagement.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operations")]
[ApiController]
[Authorize]
public class OperationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OperationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("housekeeping/complete")]
    public async Task<ActionResult<ApiResponse<bool>>> CompleteHousekeeping([FromBody] CompleteHousekeepingTaskCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new ApiResponse<bool>(result, "Housekeeping task completed successfully."));
    }

    [HttpPost("maintenance")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateMaintenance([FromBody] CreateMaintenanceRequestCommand command)
    {
        var requestId = await _mediator.Send(command);
        return Ok(new ApiResponse<Guid>(requestId, "Maintenance request logged successfully."));
    }

    [HttpPost("maintenance/resolve")]
    public async Task<ActionResult<ApiResponse<bool>>> ResolveMaintenance([FromBody] ResolveMaintenanceRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new ApiResponse<bool>(result, "Maintenance request resolved successfully."));
    }
}
