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
        var authDetails = await _userRepository.GetAuthDetailsAsync(email);

        if (authDetails == null || !passwordHasher.VerifyPassword(password, authDetails.PasswordHash))
        {
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        var accessToken = _tokenService.GenerateAccessToken(
            authDetails.UserId, 
            authDetails.Email, 
            authDetails.TenantId, 
            authDetails.Roles, 
            authDetails.Permissions, 
            authDetails.Features);

        var refreshToken = _tokenService.GenerateRefreshToken();
        var tokenHash = _tokenService.GetHashToken(refreshToken);

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = authDetails.UserId,
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

        var authDetails = await _userRepository.GetAuthDetailsByIdAsync(existingToken.UserId);
        if (authDetails == null)
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
            UserId = authDetails.UserId,
            TokenHash = newTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress
        });

        await _refreshTokenRepository.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(
            authDetails.UserId, 
            authDetails.Email, 
            authDetails.TenantId, 
            authDetails.Roles, 
            authDetails.Permissions, 
            authDetails.Features);

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
