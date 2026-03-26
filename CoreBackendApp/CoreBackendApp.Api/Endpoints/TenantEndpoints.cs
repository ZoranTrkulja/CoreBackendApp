using CoreBackendApp.Api.Common.Extensions;
using CoreBackendApp.Application.Interface;

namespace CoreBackendApp.Api.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("tenants").WithTags("Tenants").RequireAuthorization("tenants.read");
        
        group.MapGet("/", async (ITenantService tenantService) =>
        {
            var result = await tenantService.GetAllAsync();
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        return endpointRouteBuilder;
    }
}
