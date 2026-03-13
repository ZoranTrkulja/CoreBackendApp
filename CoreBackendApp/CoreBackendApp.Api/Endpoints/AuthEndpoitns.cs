using CoreBackendApp.Application.Auth;
using CoreBackendApp.Application.Interface;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Api.Common.Validation;

namespace CoreBackendApp.Api.Endpoints
{
    public static class AuthEndpoitns
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            var group = endpointRouteBuilder.MapGroup("/api/auth").WithTags("Auth");

            group.MapPost("/login", async (LoginRequest loginRequest, HttpContext httpContext, IAuthService authService) =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var result = await authService.LoginAsync(loginRequest.Email, loginRequest.Password, ipAddress);
                return Results.Ok(result);
            })
            .AddEndpointFilter<ValidationFilter<LoginRequest>>()
            .AllowAnonymous();

            group.MapPost("/refresh", async (RefreshToken refreshToken, HttpContext httpContext, IAuthService authService) =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var result =  await authService.RefreshAsync(refreshToken.TokenHash, ipAddress);
                return Results.Ok(result);
            });

            group.MapPost("/logout", async (RefreshToken refreshToken, IAuthService authService) =>
            {
                 await authService.LogoutAsync(refreshToken.TokenHash);
                return Results.Ok();
            });

            return endpointRouteBuilder;
        }
    }
}
