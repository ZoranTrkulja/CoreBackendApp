using CoreBackendApp.Application.Interface;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Repositories;

public class RoleRepository(CoreDbContext context) : IRoleRepository
{
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await context.Roles.AnyAsync(r => r.Id == id);
    }
}
