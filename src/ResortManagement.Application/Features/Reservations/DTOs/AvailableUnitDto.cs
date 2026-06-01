using System;

namespace ResortManagement.Application.Features.Reservations.DTOs;

public record AvailableUnitDto(
    Guid UnitId,
    string UnitNumber,
    Guid UnitTypeId,
    string UnitTypeName,
    decimal BasePrice,
    decimal CalculatedTotalPrice,
    int MaxOccupancy
);
