using CoreBackendApp.Application.Interface;
using CoreBackendApp.Application.Users;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Repositories
{
    public class UserRepository(CoreDbContext coreDbContext) : IUserRepository
    {
        private readonly CoreDbContext _coreDbContext = coreDbContext;

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _coreDbContext.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _coreDbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<string>> GetRolesAsync(Guid userId)
        {
            return await _coreDbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetPermissionsAsync(Guid userId)
        {
            return await _coreDbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetFeaturesAsync(Guid userId)
        {
            var user = await _coreDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Enumerable.Empty<string>();

            return await _coreDbContext.TenantFeatures
                .Where(tf => tf.TenantId == user.TenantId)
                .Select(tf => tf.Feature.Key)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserResponse>> GetAllWithRolesAsync()
        {
            return await _coreDbContext.Users
                .Select(u => new UserResponse(
                    u.Id, 
                    u.Email, 
                    u.TenantId,
                    u.UserRoles.Select(ur => ur.Role.Name)
                ))
                .ToListAsync();
        }

        public async Task<UserResponse?> GetByIdWithRolesAsync(Guid userId)
        {
            return await _coreDbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserResponse(
                    u.Id,
                    u.Email,
                    u.TenantId,
                    u.UserRoles.Select(ur => ur.Role.Name)
                ))
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(User user)
        {
            _coreDbContext.Users.Add(user);
            await _coreDbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _coreDbContext.Users.Update(user);
            await _coreDbContext.SaveChangesAsync();
        }
    }
}
