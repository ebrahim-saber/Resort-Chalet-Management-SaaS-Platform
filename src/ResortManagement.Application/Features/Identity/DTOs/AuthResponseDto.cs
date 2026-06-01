using System;

namespace ResortManagement.Application.Features.Identity.DTOs;

public record AuthResponseDto(
    string Token,
    string RefreshToken,
    DateTime Expiration
);
