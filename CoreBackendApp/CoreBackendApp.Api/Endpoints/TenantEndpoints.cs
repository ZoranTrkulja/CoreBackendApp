using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Api.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("tenants").WithTags("Tenants").RequireAuthorization("tenants.read");
        group.MapGet("/", async (CoreDbContext coreDbContext) =>
        {
            var tenants = await coreDbContext.Tenants
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    Featires = t.TenantFeatures.Select(tf => tf.Feature.Key)
                })
                .ToListAsync();

            return Results.Ok(tenants);
        });
        return endpointRouteBuilder;
    }
}
