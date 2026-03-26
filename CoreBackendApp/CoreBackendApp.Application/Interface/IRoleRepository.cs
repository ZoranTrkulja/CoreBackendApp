using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Application.Interface;

public interface IRoleRepository
{
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(Guid id);
}
