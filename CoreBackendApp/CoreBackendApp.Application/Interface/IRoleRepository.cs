namespace CoreBackendApp.Application.Interface;

public interface IRoleRepository
{
    Task<bool> ExistsAsync(Guid id);
}
