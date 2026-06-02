using System;
using System.Collections.Generic;
using ResortManagement.Domain.Entities.Identity;

namespace ResortManagement.Application.Common.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user, List<string> permissions, List<string> roles);
    string GenerateRefreshToken();
}
