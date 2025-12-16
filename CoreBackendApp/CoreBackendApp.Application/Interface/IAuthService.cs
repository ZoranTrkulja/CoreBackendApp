using CoreBackendApp.Application.Auth;

namespace CoreBackendApp.Application.Interface
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password, string ipAddress);
        Task<LoginResponse> RefreshAsync(string refreshToken, string ipAddress);
        Task LogoutAsync(string refreshToken);
    }
}
