using CoreBackendApp.Application.Interface;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Repositories;

public class TenantRepository(CoreDbContext context) : ITenantRepository
{
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await context.Tenants.AnyAsync(t => t.Id == id);
    }
}
