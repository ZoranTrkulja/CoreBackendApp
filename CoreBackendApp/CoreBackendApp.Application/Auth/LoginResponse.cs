namespace CoreBackendApp.Application.Auth
{
    public record LoginResponse(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExiresAt
     );
}
