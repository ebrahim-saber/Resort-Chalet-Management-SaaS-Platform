using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResortManagement.Application.Common.Exceptions;
using ResortManagement.Application.Common.Models;

namespace ResortManagement.WebApi.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new ApiResponse<object>();

        switch (exception)
        {
            case ValidationException valEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errors = new List<string>();
                foreach (var kvp in valEx.Errors)
                {
                    foreach (var error in kvp.Value)
                    {
                        errors.Add($"{kvp.Key}: {error}");
                    }
                }
                response = new ApiResponse<object>("Validation failed", errors);
                break;

            case NotFoundException nfEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new ApiResponse<object>("Resource not found", new List<string> { nfEx.Message });
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var errMsg = _env.IsDevelopment() ? exception.Message : "An unexpected error occurred.";
                response = new ApiResponse<object>("Internal server error", new List<string> { errMsg });
                break;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
// Middleware extension method
public static class ExceptionMiddlewareExtensions
{
    public static void UseGlobalExceptionHandling(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}
