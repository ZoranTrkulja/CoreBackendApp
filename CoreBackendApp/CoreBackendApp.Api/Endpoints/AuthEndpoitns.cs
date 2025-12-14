using CoreBackendApp.Application.Auth;

namespace CoreBackendApp.Api.Endpoints
{
    public static class AuthEndpoitns
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            var group = endpointRouteBuilder.MapGroup("/api/auth").WithTags("Auth");

            group.MapPost("/login", async (LoginRequest loginRequest, AuthService authService) =>
            {
                var result = await authService.LoginAsync(loginRequest);
                return Results.Ok(result);
            })
            .AllowAnonymous();

            group.MapPost("/refresh", () =>
            {
                return Results.BadRequest("Not implemented yet");
            });

            group.MapPost("/logout", () =>
            {
                return Results.BadRequest("Not implemented yet");
            });

            return endpointRouteBuilder;
        }
    }
}
