using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Api.Endpoints;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("roles").WithTags("Roles").RequireAuthorization();

        group.MapGet("/", async (CoreDbContext coreDbContext) =>
        {
            var roles = await coreDbContext.Roles
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    Permissions = r.RolePermissions!.Select(rp => new
                    {
                        rp.Permission.Code
                    })
                })
                .ToListAsync();
            return Results.Ok(roles);
        });

        group.MapGet("/{id:guid}", async (Guid id, CoreDbContext coreDbContext) =>
        {
            var role = await coreDbContext.Roles
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    Permissions = r.RolePermissions!.Select(rp => new
                    {
                        rp.Permission.Code
                    })
                })
                .FirstOrDefaultAsync();
            return Results.Ok(role);
        });
        return endpointRouteBuilder;
    }
}
