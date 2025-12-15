using CoreBackendApp.Application.Auth;

namespace CoreBackendApp.Application.Interface
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
    }
}
