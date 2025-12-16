using CoreBackendApp.Application.Interface;
using CoreBackendApp.Domain.Entities;
using System.Security;

namespace CoreBackendApp.Application.Auth
{
    public class AuthService(IUserRepository userRepository, TokenService tokenService, IRefreshTokenRepository refreshTokenRepository) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly TokenService _tokenService = tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

        public async Task<LoginResponse> LoginAsync(string email, string password, string ipAddress)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = await _userRepository.GetRolesAsync(user.Id);
            var permissions = await _userRepository.GetPermissionsAsync(user.Id);

            var features = await _userRepository.GetFeaturesAsync(user.Id);

            var accessToken = _tokenService.GenerateAccessToken(
                user,
                roles,
                permissions,
                features);

            var refreshToken = _tokenService.GenerateRefreshToken();
            var tokenHash = _tokenService.GetHashToken(refreshToken);

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExipresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress
            });

            return new LoginResponse(accessToken, refreshToken);
        }

        public async Task<LoginResponse> RefreshAsync(string refreshToken, string ipAddress)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken) ?? throw new SecurityException("Invalid refresh token"); ;
            
            if(!existingToken.IsActive)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            var user = await _userRepository.GetByIdAsync(existingToken.UserId) ??
                throw new UnauthorizedAccessException("User not found.");

            existingToken.RevokedAt = DateTime.UtcNow;
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var newTokenHash = _tokenService.GetHashToken(newRefreshToken);

            existingToken.ReplacedByTokenHash = newTokenHash;


            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                Id= Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = newTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExipresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress
            });

            var roles = await _userRepository.GetRolesAsync(user.Id);
            var permissions = await _userRepository.GetPermissionsAsync(user.Id);
            var features = await _userRepository.GetFeaturesAsync(user.Id);

            var accessToken = _tokenService.GenerateAccessToken(
                user,
                roles,
                permissions,
                features);

            return new LoginResponse(accessToken, newRefreshToken);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken) ??
                throw new SecurityException("Invalid refresh token");
            if (!existingToken.IsActive)
                throw new UnauthorizedAccessException("Invalid refresh token.");
            existingToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.SaveChangesAsync();
        }
    }
}
