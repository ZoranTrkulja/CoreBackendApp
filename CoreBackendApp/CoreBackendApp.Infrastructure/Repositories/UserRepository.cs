using CoreBackendApp.Application.Interface;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Repositories
{
    public class UserRepository(CoreDbContext coreDbContext) : IUserRepository
    {
        private readonly CoreDbContext _coreDbContext = coreDbContext;

        public async Task<User?> GetUserWithDetailsAsync(string email)
        {
            return await _coreDbContext.Users
                .Include(x => x.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(x => x.Tenant)
                    .ThenInclude(t => t.TenantFeatures)
                        .ThenInclude(tf => tf.Feature)
                .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted)!;
        }
    }
}
