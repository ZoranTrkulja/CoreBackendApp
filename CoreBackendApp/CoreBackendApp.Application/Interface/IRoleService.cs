using CoreBackendApp.Application.Common.Models;

namespace CoreBackendApp.Application.Interface;

public record RoleResponse(Guid Id, string Name, IEnumerable<string> Permissions);

public interface IRoleService
{
    Task<Result<IEnumerable<RoleResponse>>> GetAllAsync();
    Task<Result<RoleResponse>> GetByIdAsync(Guid id);
}
