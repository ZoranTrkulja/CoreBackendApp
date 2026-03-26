using CoreBackendApp.Api.Common.Extensions;
using CoreBackendApp.Application.Interface;

namespace CoreBackendApp.Api.Endpoints;

public static class FeatureEndpoints
{
    public static IEndpointRouteBuilder MapFeatureEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("features").WithTags("Features").RequireAuthorization("features.read");

        group.MapGet("/", async (IFeatureService featureService) =>
        {
            var result = await featureService.GetAllAsync();
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        return endpointRouteBuilder;
    }
}
