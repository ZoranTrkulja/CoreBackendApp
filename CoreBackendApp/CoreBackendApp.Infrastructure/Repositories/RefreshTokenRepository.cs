using CoreBackendApp.Application.Interface;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);

            return await _coreDbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
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

        private string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }
    }   
}
