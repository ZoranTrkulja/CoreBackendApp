using CoreBackendApp.Application.Auth;
using CoreBackendApp.Application.Common.Models;

namespace CoreBackendApp.Application.Interface;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(string email, string password, string ipAddress);
    Task<Result<LoginResponse>> RefreshAsync(string refreshToken, string ipAddress);
    Task<Result> LogoutAsync(string refreshToken);
}
