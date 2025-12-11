using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Application.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetUserWithDetailsAsync(string email);
    }
}
