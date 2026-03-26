using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CoreBackendApp.Api.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next, 
    ILogger<GlobalExceptionMiddleware> logger, 
    IHostEnvironment hostEnvironment)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;
    private readonly IHostEnvironment _env = hostEnvironment;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", traceId);

            var problemDetails = CreateProblemDetails(context, ex, traceId);

            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception ex, string traceId)
    {
        var (statusCode, title, detail) = ex switch
        {
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized, 
                "Unauthorized", 
                "You are not authorized to access this resource."),
            
            KeyNotFoundException => (
                StatusCodes.Status404NotFound, 
                "Not Found", 
                ex.Message),
            
            ArgumentException or InvalidOperationException => (
                StatusCodes.Status400BadRequest, 
                "Bad Request", 
                ex.Message),
            
            _ => (
                StatusCodes.Status500InternalServerError, 
                "Internal Server Error", 
                _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;

        if (_env.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = ex.ToString();
        }

        return problemDetails;
    }
}
