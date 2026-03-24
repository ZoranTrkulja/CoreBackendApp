using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Api.Endpoints;

public static class PermissionEndpoints
{
    public static IEndpointRouteBuilder MapPermissionEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("permissions").WithTags("Permissions").RequireAuthorization("permissions.read");

        group.MapGet("/", async (CoreDbContext coreDbContext) =>
        {
            var permissions = await coreDbContext.Permissions
                .Select(p => new
                {
                    p.Id,
                    p.Code,
                    p.Description
                })
                .ToListAsync();
            return Results.Ok(permissions);
        });

        group.MapGet("/{id:guid}", async (Guid id, CoreDbContext coreDbContext) =>
        {
            var permission = await coreDbContext.Permissions
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Code,
                    p.Description
                })
                .FirstOrDefaultAsync();
            return Results.Ok(permission);
        });
        return endpointRouteBuilder;
    }
}
