using CoreBackendApp.Application.Interface;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Repositories;

public class PermissionRepository(CoreDbContext context) : IPermissionRepository
{
    private readonly CoreDbContext _context = context;

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Permissions.AsNoTracking().ToListAsync();
    }

    public async Task<Permission?> GetByIdAsync(Guid id)
    {
        return await _context.Permissions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }
}
