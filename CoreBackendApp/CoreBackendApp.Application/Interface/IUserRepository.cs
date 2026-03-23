using CoreBackendApp.Application.Users;
using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Application.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<string>> GetRolesAsync(Guid userId);
        Task<IEnumerable<string>> GetPermissionsAsync(Guid userId);
        Task<IEnumerable<string>> GetFeaturesAsync(Guid userId);
        
        Task<IEnumerable<UserResponse>> GetAllWithRolesAsync();
        Task<UserResponse?> GetByIdWithRolesAsync(Guid userId);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
