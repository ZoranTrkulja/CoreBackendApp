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

        public async Task<RefreshToken?> GetByTokenAsync(string refreshToken)
        {
            // Note: This is slow because it hashes every time. 
            // In a real app, you'd find by UserId or an Id embedded in the token string itself.
            var tokens = await _coreDbContext.RefreshTokens
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();

            return tokens.FirstOrDefault(rt => BCrypt.Net.BCrypt.Verify(refreshToken, rt.TokenHash));
        }

        public async Task RevokeAllForUserAsync(Guid userId)
        {
            var activeTokens = await _coreDbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _coreDbContext.SaveChangesAsync();
        }
    }   
}
