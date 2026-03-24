using CoreBackendApp.Application.Interface;
using CoreBackendApp.Application.Users;
using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace CoreBackendApp.Infrastructure.Repositories
{
    public class UserRepository(CoreDbContext coreDbContext) : IUserRepository
    {
        private readonly CoreDbContext _coreDbContext = coreDbContext;

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _coreDbContext.Users
                .IgnoreQueryFilters()
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _coreDbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<string>> GetRolesAsync(Guid userId)
        {
            return await _coreDbContext.UserRoles
                .IgnoreQueryFilters()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetPermissionsAsync(Guid userId)
        {
            return await _coreDbContext.UserRoles
                .IgnoreQueryFilters()
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetFeaturesAsync(Guid userId)
        {
            var user = await _coreDbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null) return Enumerable.Empty<string>();

            return await _coreDbContext.TenantFeatures
                .IgnoreQueryFilters()
                .Where(tf => tf.TenantId == user.TenantId)
                .Select(tf => tf.Feature.Key)
                .ToListAsync();
        }

        public async Task<PagedList<UserResponse>> GetAllWithRolesAsync(PaginationParams paginationParams)
        {
            var query = _coreDbContext.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                query = query.Where(u => u.Email.Contains(paginationParams.SearchTerm));
            }

            var count = await query.CountAsync();
            var items = await query
                .ProjectToType<UserResponse>()
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedList<UserResponse>(items, count, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<UserResponse?> GetByIdWithRolesAsync(Guid userId)
        {
            return await _coreDbContext.Users
                .Where(u => u.Id == userId)
                .ProjectToType<UserResponse>()
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
