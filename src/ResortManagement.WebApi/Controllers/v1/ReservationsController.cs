using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Models;
using ResortManagement.Application.Features.Reservations.DTOs;
using ResortManagement.Application.Features.Reservations.Queries.GetAvailableUnits;
using ResortManagement.Application.Features.Reservations.Commands.CreateReservation;

namespace ResortManagement.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reservations")]
[ApiController]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("availability")]
    public async Task<ActionResult<ApiResponse<List<AvailableUnitDto>>>> GetAvailability(
        [FromQuery] DateTime checkInDate, 
        [FromQuery] DateTime checkOutDate, 
        [FromQuery] Guid resortId)
    {
        var query = new GetAvailableUnitsQuery(checkInDate, checkOutDate, resortId);
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse<List<AvailableUnitDto>>(result, "Availability list resolved successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateReservationCommand command)
    {
        var reservationId = await _mediator.Send(command);
        return Ok(new ApiResponse<Guid>(reservationId, "Reservation created successfully."));
    }
}
