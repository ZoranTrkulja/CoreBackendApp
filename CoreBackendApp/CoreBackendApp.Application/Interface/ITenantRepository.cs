namespace CoreBackendApp.Application.Interface;

public interface ITenantRepository
{
    Task<bool> ExistsAsync(Guid id);
}
