using CoreBackendApp.Application.Interface;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Repositories
{
    public class UserRepository(CoreDbContext coreDbContext) : IUserRepository
    {
        private readonly CoreDbContext _coreDbContext = coreDbContext;

        public async Task<User?> GetByIdAsync(Guid userId) => await _coreDbContext.Users.FirstOrDefaultAsync(u => Guid.Equals(u.Id, userId));
        public async Task<User?> GetByEmailAsync(string email) => await _coreDbContext.Users.FirstOrDefaultAsync(u => string.Equals(u.Email, email));

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
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetFeaturesAsync(Guid userId)
        {
            var tenantId = await _coreDbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => u.TenantId)
                .FirstOrDefaultAsync();
            return await _coreDbContext.TenantFeatures
                .Where(tf => tf.TenantId == tenantId)
                .Select(tf => tf.Feature.Key)
                .ToListAsync();
        }
    }
}
