using CoreBackendApp.Api.Common.Extensions;
using CoreBackendApp.Application.Interface;

namespace CoreBackendApp.Api.Endpoints;

public static class PermissionEndpoints
{
    public static IEndpointRouteBuilder MapPermissionEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("permissions").WithTags("Permissions").RequireAuthorization("permissions.read");

        group.MapGet("/", async (IPermissionService permissionService) =>
        {
            var result = await permissionService.GetAllAsync();
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        group.MapGet("/{id:guid}", async (Guid id, IPermissionService permissionService) =>
        {
            var result = await permissionService.GetByIdAsync(id);
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        return endpointRouteBuilder;
    }
}
