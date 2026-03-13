using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Api.Endpoints;

public static class FeatureEndpoints
{
    public static IEndpointRouteBuilder MapFeatureEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("features").WithTags("Features").RequireAuthorization();

        group.MapGet("/", async (CoreDbContext coreDbContext) =>
        {
            var features = await coreDbContext.Features.ToListAsync();
            return Results.Ok(features);
        });
        return endpointRouteBuilder;
    }
}
