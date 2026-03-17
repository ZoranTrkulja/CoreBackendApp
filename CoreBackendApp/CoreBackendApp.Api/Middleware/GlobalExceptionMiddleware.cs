using Microsoft.AspNetCore.Mvc;

namespace CoreBackendApp.Api.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment hostEnvironment)
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
            _logger.LogError(ex, "An error occurred: {Message}", ex.Message);

            var (statusCode, title) = ex switch
            {
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = _env.IsDevelopment() ? ex.ToString() : ex.Message,
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
