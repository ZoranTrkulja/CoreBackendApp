namespace CoreBackendApp.Application.Auth
{
    public class DTOs
    {
        public record LoginRequest(string Email, string Password);
        public record LoginResponse(string AccessToken, string RefreshToken);
        public record RefreshTokenRequest(string RefreshToken);
    }
}
