using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Interface;

namespace CoreBackendApp.Application.Services;

public class RoleService(IRoleRepository roleRepository) : IRoleService
{
    private readonly IRoleRepository _roleRepository = roleRepository;

    public async Task<Result<IEnumerable<RoleResponse>>> GetAllAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        
        var response = roles.Select(r => new RoleResponse(
            r.Id, 
            r.Name, 
            r.RolePermissions!.Select(rp => rp.Permission.Code)));

        return Result.Success(response);
    }

    public async Task<Result<RoleResponse>> GetByIdAsync(Guid id)
    {
        var role = await _roleRepository.GetByIdAsync(id);

        if (role == null)
        {
            return Result.Failure<RoleResponse>(Error.NotFound("Role.NotFound", "Role not found."));
        }

        var response = new RoleResponse(
            role.Id, 
            role.Name, 
            role.RolePermissions!.Select(rp => rp.Permission.Code));

        return Result.Success(response);
    }
}
