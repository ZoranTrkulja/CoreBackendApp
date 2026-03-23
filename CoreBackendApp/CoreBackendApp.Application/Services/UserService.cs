using CoreBackendApp.Application.Common.Interfaces;
using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Interface;
using CoreBackendApp.Application.Users;
using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Application.Services;

public class UserService(
    IUserRepository userRepository, 
    ITenantRepository tenantRepository,
    IPasswordHasher passwordHasher) : IUserService
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
        if (!await tenantRepository.ExistsAsync(request.TenantId))
        {
            return Result.Failure<Guid>(UserErrors.InvalidTenant);
        }

        if (await userRepository.GetByEmailAsync(request.Email) != null)
        {
            return Result.Failure<Guid>(UserErrors.EmailAlreadyExists);
        }

        var user = User.Create(
            request.Email,
            passwordHasher.HashPassword(request.Password),
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
