using CoreBackendApp.Application.Common.Interfaces;
using Serilog.Context;

namespace CoreBackendApp.Api.Middleware;

public class LogEnrichmentMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        var tenantId = tenantProvider.TenantId;
        var correlationId = context.TraceIdentifier;

        using (LogContext.PushProperty("TenantId", tenantId))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
