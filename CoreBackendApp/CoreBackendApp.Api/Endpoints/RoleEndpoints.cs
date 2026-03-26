using CoreBackendApp.Api.Common.Extensions;
using CoreBackendApp.Application.Interface;

namespace CoreBackendApp.Api.Endpoints;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("roles").WithTags("Roles").RequireAuthorization("roles.read");

        group.MapGet("/", async (IRoleService roleService) =>
        {
            var result = await roleService.GetAllAsync();
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        group.MapGet("/{id:guid}", async (Guid id, IRoleService roleService) =>
        {
            var result = await roleService.GetByIdAsync(id);
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        return endpointRouteBuilder;
    }
}
