using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Application.Interface
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId);
        Task<RefreshToken?> GetByTokenAsync(string refreshToken);
        public Task SaveChangesAsync();
    }
}
