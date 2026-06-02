using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Domain.Entities.Identity;

namespace ResortManagement.Infrastructure.Services;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user, List<string> permissions, List<string> roles)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? "super_secret_resort_management_saas_key_12345!";
        var issuer = _configuration["Jwt:Issuer"] ?? "ResortManagementSaaS";
        var audience = _configuration["Jwt:Audience"] ?? "ResortManagementClients";
        var expirationMinutes = double.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("FullName", user.FullName),
            new Claim("TenantId", user.TenantId.ToString())
        };

        // Inject permissions as claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        // Inject roles as standard claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
