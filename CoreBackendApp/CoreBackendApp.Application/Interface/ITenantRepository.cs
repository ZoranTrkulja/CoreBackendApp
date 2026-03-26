using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Application.Interface;

public interface ITenantRepository
{
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Tenant>> GetAllAsync();
}
