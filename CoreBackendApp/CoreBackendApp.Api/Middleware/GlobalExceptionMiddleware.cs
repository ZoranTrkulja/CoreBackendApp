using Microsoft.AspNetCore.Mvc;

namespace CoreBackendApp.Api.Middleware
{
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
                _logger.LogError(ex, "An unhandled exception occurred.");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var problem = new ProblemDetails
                {
                    Status = 500,
                    Title = "Internal Server Error",
                    Detail = _env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path
                };
                problem.Extensions["traceId"] = context.TraceIdentifier;

                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}
