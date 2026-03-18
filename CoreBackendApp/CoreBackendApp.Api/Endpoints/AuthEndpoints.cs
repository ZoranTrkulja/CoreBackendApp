using CoreBackendApp.Application.Auth;
using CoreBackendApp.Application.Interface;
using CoreBackendApp.Api.Common.Validation;
using CoreBackendApp.Api.Common.Extensions;

namespace CoreBackendApp.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest loginRequest, HttpContext httpContext, IAuthService authService) =>
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var result = await authService.LoginAsync(loginRequest.Email, loginRequest.Password, ipAddress);
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        })
        .AddEndpointFilter<ValidationFilter<LoginRequest>>()
        .AllowAnonymous();

        group.MapPost("/refresh", async (RefreshTokenRequest refreshTokenRequest, HttpContext httpContext, IAuthService authService) =>
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result =  await authService.RefreshAsync(refreshTokenRequest.RefreshToken, ipAddress);
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        group.MapPost("/logout", async (RefreshTokenRequest refreshTokenRequest, IAuthService authService) =>
        {
            var result = await authService.LogoutAsync(refreshTokenRequest.RefreshToken);
            
            return result.IsSuccess 
                ? Results.Ok() 
                : result.ToProblemDetails();
        });

        return endpointRouteBuilder;
    }
}
