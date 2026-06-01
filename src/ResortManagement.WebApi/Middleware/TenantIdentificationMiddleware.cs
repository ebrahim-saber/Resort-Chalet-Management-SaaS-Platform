using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ResortManagement.Application.Common.Interfaces;

namespace ResortManagement.WebApi.Middleware;

public class TenantIdentificationMiddleware
{
    private readonly RequestDelegate _next;

    public TenantIdentificationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        // Try extracting from JWT Claim first (user is authenticated)
        var tenantClaim = context.User?.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;

        if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var claimTenantId))
        {
            tenantProvider.SetTenantId(claimTenantId);
        }
        // Fallback to HTTP Custom Header 'X-Tenant-Id' (e.g. for guest bookings or public pages)
        else if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue) &&
                 Guid.TryParse(headerValue.ToString(), out var headerTenantId))
        {
            tenantProvider.SetTenantId(headerTenantId);
        }

        await _next(context);
    }
}

public static class TenantMiddlewareExtensions
{
    public static void UseTenantIdentification(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        app.UseMiddleware<TenantIdentificationMiddleware>();
    }
}
