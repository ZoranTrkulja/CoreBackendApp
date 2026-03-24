using CoreBackendApp.Application.Common.Interfaces;
using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Interface;
using CoreBackendApp.Application.Users;
using CoreBackendApp.Domain.Entities;

using Microsoft.Extensions.Logging;

namespace CoreBackendApp.Application.Services;

public class UserService(
    IUserRepository userRepository, 
    ITenantRepository tenantRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    ILogger<UserService> logger) : IUserService
{
    public async Task<PagedList<UserResponse>> GetAllAsync(PaginationParams paginationParams)
    {
        logger.LogInformation("Retrieving paged users with params: {@PaginationParams}", paginationParams);
        return await userRepository.GetAllWithRolesAsync(paginationParams);
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdWithRolesAsync(id);

        if (user == null)
        {
            logger.LogWarning("User with ID {UserId} not found", id);
            return Result.Failure<UserResponse>(UserErrors.NotFound);
        }

        return user;
    }

    public async Task<Result<Guid>> CreateAsync(CreateUserRequest request)
    {
        if (!await tenantRepository.ExistsAsync(request.TenantId))
        {
            logger.LogWarning("Create user failed: Tenant {TenantId} not found", request.TenantId);
            return Result.Failure<Guid>(UserErrors.InvalidTenant);
        }

        if (await userRepository.GetByEmailAsync(request.Email) != null)
        {
            logger.LogWarning("Create user failed: Email {Email} already exists", request.Email);
            return Result.Failure<Guid>(UserErrors.EmailAlreadyExists);
        }

        var user = User.Create(
            request.Email,
            passwordHasher.HashPassword(request.Password),
            request.TenantId);

        await userRepository.AddAsync(user);

        logger.LogInformation("User created successfully with ID {UserId} for Tenant {TenantId}", user.Id, user.TenantId);

        return user.Id;
    }

    public async Task<Result> AssignRoleAsync(Guid userId, Guid roleId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("Role assignment failed: User {UserId} not found", userId);
            return Result.Failure(UserErrors.NotFound);
        }

        if (!await roleRepository.ExistsAsync(roleId))
        {
            logger.LogWarning("Role assignment failed: Role {RoleId} not found", roleId);
            return Result.Failure(UserErrors.RoleNotFound);
        }

        if (user.UserRoles.Any(ur => ur.RoleId == roleId))
        {
            logger.LogWarning("Role assignment failed: User {UserId} already has role {RoleId}", userId, roleId);
            return Result.Failure(UserErrors.UserAlreadyHasRole);
        }

        user.AssignRole(roleId);

        await userRepository.UpdateAsync(user);

        logger.LogInformation("Role {RoleId} assigned successfully to User {UserId}", roleId, userId);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null)
        {
            logger.LogWarning("Delete failed: User {UserId} not found", id);
            return Result.Failure(UserErrors.NotFound);
        }

        await userRepository.Delete(user);
        logger.LogInformation("User {UserId} soft-deleted successfully", id);

        return Result.Success();
    }
}
