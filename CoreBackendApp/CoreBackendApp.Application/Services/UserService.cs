using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Interface;
using CoreBackendApp.Application.Users;
using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        return await userRepository.GetAllWithRolesAsync();
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdWithRolesAsync(id);

        if (user == null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound);
        }

        return user;
    }

    public async Task<Result<Guid>> CreateAsync(CreateUserRequest request)
    {
        // Note: Password hashing should ideally be in a separate service, 
        // but keeping it here for consistency with the previous direct context usage.
        var user = User.Create(
            request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Password),
            request.TenantId);

        await userRepository.AddAsync(user);

        return user.Id;
    }

    public async Task<Result> AssignRoleAsync(Guid userId, Guid roleId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        user.AssignRole(roleId);

        await userRepository.UpdateAsync(user);

        return Result.Success();
    }
}
