using CoreBackendApp.Application.Interface;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Repositories;

public class FeatureRepository(CoreDbContext context) : IFeatureRepository
{
    private readonly CoreDbContext _context = context;

    public async Task<IEnumerable<Feature>> GetAllAsync()
    {
        return await _context.Features.AsNoTracking().ToListAsync();
    }
}
