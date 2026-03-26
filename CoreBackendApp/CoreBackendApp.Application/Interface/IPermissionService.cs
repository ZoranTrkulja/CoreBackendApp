using CoreBackendApp.Application.Common.Models;

namespace CoreBackendApp.Application.Interface;

public record PermissionResponse(Guid Id, string Code, string Description);

public interface IPermissionService
{
    Task<Result<IEnumerable<PermissionResponse>>> GetAllAsync();
    Task<Result<PermissionResponse>> GetByIdAsync(Guid id);
}
