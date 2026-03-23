using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Users;

namespace CoreBackendApp.Application.Interface;

public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllAsync();
    Task<Result<UserResponse>> GetByIdAsync(Guid id);
    Task<Result<Guid>> CreateAsync(CreateUserRequest request);
    Task<Result> AssignRoleAsync(Guid userId, Guid roleId);
}
