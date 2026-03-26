using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Interface;

namespace CoreBackendApp.Application.Services;

public class PermissionService(IPermissionRepository permissionRepository) : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository = permissionRepository;

    public async Task<Result<IEnumerable<PermissionResponse>>> GetAllAsync()
    {
        var permissions = await _permissionRepository.GetAllAsync();
        
        var response = permissions.Select(p => new PermissionResponse(p.Id, p.Code, p.Description));

        return Result.Success(response);
    }

    public async Task<Result<PermissionResponse>> GetByIdAsync(Guid id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);

        if (permission == null)
        {
            return Result.Failure<PermissionResponse>(Error.NotFound("Permission.NotFound", "Permission not found."));
        }

        var response = new PermissionResponse(permission.Id, permission.Code, permission.Description);

        return Result.Success(response);
    }
}
