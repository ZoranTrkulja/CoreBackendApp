using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Application.Interface;

public interface IPermissionRepository
{
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<Permission?> GetByIdAsync(Guid id);
}
