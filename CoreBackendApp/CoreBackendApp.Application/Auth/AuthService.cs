using CoreBackendApp.Application.Interface;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Common.Interfaces;

namespace CoreBackendApp.Application.Auth;

public class AuthService(
    IUserRepository userRepository, 
    TokenService tokenService, 
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly TokenService _tokenService = tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

    public async Task<Result<LoginResponse>> LoginAsync(string email, string password, string ipAddress)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null || !passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        var roles = await _userRepository.GetRolesAsync(user.Id);
        var permissions = await _userRepository.GetPermissionsAsync(user.Id);
        var features = await _userRepository.GetFeaturesAsync(user.Id);

        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions, features);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var tokenHash = _tokenService.GetHashToken(refreshToken);

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress
        });

        return new LoginResponse(accessToken, refreshToken);
    }

    public async Task<Result<LoginResponse>> RefreshAsync(string refreshToken, string ipAddress)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        
        if (existingToken == null)
        {
            return Result.Failure<LoginResponse>(AuthErrors.InvalidToken);
        }

        // --- REUSE DETECTION ---
        if (existingToken.RevokedAt != null)
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(existingToken.UserId);
            return Result.Failure<LoginResponse>(AuthErrors.TokenReused);
        }

        if (existingToken.IsExpired)
        {
            return Result.Failure<LoginResponse>(AuthErrors.TokenExpired);
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId);
        if (user == null)
        {
            return Result.Failure<LoginResponse>(AuthErrors.UserNotFound);
        }

        // --- TOKEN ROTATION ---
        existingToken.RevokedAt = DateTime.UtcNow;
        
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newTokenHash = _tokenService.GetHashToken(newRefreshToken);

        existingToken.ReplacedByTokenHash = newTokenHash;

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress
        });

        await _refreshTokenRepository.SaveChangesAsync();

        var roles = await _userRepository.GetRolesAsync(user.Id);
        var permissions = await _userRepository.GetPermissionsAsync(user.Id);
        var features = await _userRepository.GetFeaturesAsync(user.Id);

        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions, features);

        return new LoginResponse(accessToken, newRefreshToken);
    }

    public async Task<Result> LogoutAsync(string refreshToken)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        
        if (existingToken == null || !existingToken.IsActive)
        {
            return Result.Failure(AuthErrors.InvalidToken);
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.SaveChangesAsync();

        return Result.Success();
    }
}
