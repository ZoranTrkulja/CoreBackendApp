using CoreBackendApp.Application.Interface;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Repositories
{
    public class RefreshTokenRepository(CoreDbContext coreDbContext) : IRefreshTokenRepository
    {
        private readonly CoreDbContext _coreDbContext = coreDbContext;

        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _coreDbContext.RefreshTokens.AddAsync(refreshToken);
            await _coreDbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId)
        {
            return await _coreDbContext.RefreshTokens
                .OrderByDescending(rt => rt.CreatedAt)
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.IsActive);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string refreshToken) => await _coreDbContext.RefreshTokens.OrderByDescending(rt => rt.CreatedAt).FirstOrDefaultAsync(rt => BCrypt.Net.BCrypt.Verify(refreshToken, rt.TokenHash));

        public async Task SaveChangesAsync()
        {
            _coreDbContext.SaveChanges();
        }
    }   
}
