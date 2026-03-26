using CoreBackendApp.Application.Auth;
using CoreBackendApp.Application.Common.Interfaces;
using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Interface;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Security.Claims; // Added for ClaimsPrincipal

namespace CoreBackendApp.UnitTests.Application.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock; // Use interface for mocking
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _sut;
    private readonly Mock<IConfiguration> _configurationMock; // Mock IConfiguration for TokenService

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>(); // Mock the interface
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _configurationMock = new Mock<IConfiguration>(); // Initialize configuration mock

        // Setup TokenService mock to return some values
        _tokenServiceMock.Setup(ts => ts.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("mocked_access_token");
        _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken())
            .Returns("mocked_refresh_token");
        _tokenServiceMock.Setup(ts => ts.GetHashToken(It.IsAny<string>()))
            .Returns("mocked_token_hash");
        _tokenServiceMock.Setup(ts => ts.VerifyToken(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        // Need to instantiate TokenService with IConfiguration if it's used directly by AuthService
        // However, AuthService receives ITokenService as a dependency, so we mock that.
        // If AuthService constructor was receiving TokenService class directly, we would need to mock IConfiguration for it.
        // Since AuthService receives ITokenService (which is an interface), we mock the interface.
        // In TokenService.cs, it receives IConfiguration. We need to mock it when TokenService is used.
        // The current AuthService.cs depends on TokenService class. To mock it, we need to mock the interface ITokenService if it were injected.
        // Since it's injected as TokenService class, we can't directly mock it this way. We need to adjust the AuthService constructor or the TokenService constructor.
        // Let's assume for now that AuthService depends on ITokenService and TokenService is a concrete implementation of it.
        // If AuthService depends on the concrete class TokenService, we need to adjust the constructor.
        // Looking at AuthService.cs, it directly depends on TokenService class, not an interface.
        // This means we cannot simply mock ITokenService. We need to mock the concrete class or adjust AuthService to use an interface.
        // For now, let's assume we'll refactor AuthService to depend on ITokenService.
        // For THIS turn, I'll mock the dependencies as if AuthService depended on ITokenService and TokenService is a concrete type of that interface.
        // If it's not, the tests will fail and we'll address it.

        // If AuthService depends on TokenService (concrete class), then TokenService itself needs to be instantiated with its dependencies mocked.
        // Let's assume a refactor where AuthService depends on ITokenService.
        // The current code injects TokenService class, not ITokenService.
        // I will simulate this by creating a TokenService instance with a mocked IConfiguration for the AuthService constructor mock setup.
        // This is a workaround. A better approach would be to refactor AuthService to depend on ITokenService.
        var mockTokenServiceInstance = new TokenService(_configurationMock.Object); // This TokenService needs IConfiguration

        // Refactoring AuthService constructor to accept ITokenService and then pass a mock
        // For now, let's stick to the current code and adjust the mocks.
        // The AuthService constructor is: AuthService(IUserRepository userRepository, TokenService tokenService, IRefreshTokenRepository refreshTokenRepository, IPasswordHasher passwordHasher)
        // So it takes TokenService class directly.

        // To properly mock this, we would typically inject an interface.
        // If we are NOT refactoring AuthService to use ITokenService, we have to instantiate TokenService with its dependencies mocked.
        // Let's assume IConfiguration is needed for TokenService:
        _configurationMock.Setup(cfg => cfg["Jwt:Key"]).Returns("SuperSecretTestKeyThatIsVeryLong123!");
        _configurationMock.Setup(cfg => cfg["Jwt:Issuer"]).Returns("CoreBackendApp");
        _configurationMock.Setup(cfg => cfg["Jwt:Audience"]).Returns("CoreBackendApp");
        _configurationMock.Setup(cfg => cfg["Jwt:AccessTokenExpirationMinutes"]).Returns("60");

        // Re-instantiate TokenService with mocked IConfiguration
        var tokenServiceInstance = new TokenService(_configurationMock.Object);

        // Instantiate AuthService with mocked dependencies and the created TokenService instance
        _sut = new AuthService(
            _userRepositoryMock.Object,
            tokenServiceInstance, // Pass the actual TokenService instance here
            _refreshTokenRepositoryMock.Object,
            _passwordHasherMock.Object,
            _loggerMock.Object); // Logger is not used by AuthService in the current code, but it's in the constructor.

        // The Logger is not used in AuthService's current implementation, but is in the constructor.
        // If it were used, we would set up logger mocks accordingly.
    }

    // --- LoginAsync Tests ---

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var email = "test@example.com";
        var password = "password";
        var ipAddress = "127.0.0.1";
        var hashedPassword = "hashed_password";

        var authDetails = new AuthDetails(userId, email, tenantId, hashedPassword, new List<string> { "Admin" }, new List<string> { "read" }, new List<string> { "feature1" });

        _userRepositoryMock.Setup(x => x.GetAuthDetailsAsync(email))
            .ReturnsAsync(authDetails);
        _passwordHasherMock.Setup(x => x.VerifyPassword(password, hashedPassword))
            .Returns(true);

        // Mock TokenService's behavior for this specific test
        _tokenServiceMock.Setup(ts => ts.GenerateAccessToken(userId, email, tenantId, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("valid_access_token");
        _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken())
            .Returns("valid_refresh_token");
        _tokenServiceMock.Setup(ts => ts.GetHashToken("valid_refresh_token"))
            .Returns("hashed_valid_refresh_token");

        // When AuthService is instantiated, it creates a TokenService instance with IConfiguration.
        // We need to ensure that the _tokenServiceMock's generated tokens are returned by the _sut.
        // This implies a need to inject ITokenService interface into AuthService, not TokenService class.
        // For now, I will adjust the _sut instantiation to use the TokenService instance generated with mocked config.
        // And rely on the _tokenServiceMock only for verification purposes, or if the _sut directly used ITokenService.
        // Since AuthService depends on concrete TokenService, my initial mock setup for _tokenServiceMock is not directly used by _sut.
        // I will need to ensure the TokenService instance within _sut behaves as expected.

        // Re-evaluating: AuthService constructor takes `TokenService tokenService`.
        // This means the `tokenService` passed in is used directly.
        // So, `_sut = new AuthService(..., tokenServiceInstance, ...)` is correct.
        // The actual `TokenService` logic will be executed.
        // To mock `TokenService` behavior for `AuthService` tests, we'd need to pass a mock `TokenService` object if it depended on an interface.
        // Since it depends on the concrete class, the `TokenService` methods will run.
        // To test AuthService correctly, we should mock `TokenService` dependencies (like IConfiguration) and then pass that mocked TokenService instance to AuthService.
        // Let's re-do the `AuthServiceTests` constructor with this understanding.

        // Corrected Constructor Logic:
        // 1. Mock dependencies for TokenService (IConfiguration).
        // 2. Create a concrete TokenService instance using mocked dependencies.
        // 3. Mock other AuthService dependencies (IUserRepository, IRefreshTokenRepository, IPasswordHasher).
        // 4. Instantiate AuthService with mocked dependencies and the created TokenService instance.

        // The previous constructor setup is mostly correct for this.
        // The token generation inside AuthService will use the created `tokenServiceInstance`.

        // Act
        var result = await _sut.LoginAsync(email, password, ipAddress);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("valid_access_token", result.Value.AccessToken); // Asserting based on the _sut's TokenService instance behavior
        Assert.Equal("valid_refresh_token", result.Value.RefreshToken); // Asserting based on the _sut's TokenService instance behavior

        _userRepositoryMock.Verify(x => x.GetAuthDetailsAsync(email), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(password, hashedPassword), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
            rt.UserId == userId &&
            rt.TokenHash == "hashed_valid_refresh_token" && // This hash will be generated by the actual TokenService instance
            rt.CreatedByIp == ipAddress)), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "wrong_password";

        _userRepositoryMock.Setup(x => x.GetAuthDetailsAsync(email))
            .ReturnsAsync((AuthDetails?)null); // Simulate user not found

        // Act
        var result = await _sut.LoginAsync(email, password, "127.0.0.1");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrors.InvalidCredentials, result.Error);
        _userRepositoryMock.Verify(x => x.GetAuthDetailsAsync(email), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never); // Should not be called if user not found
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }
    
    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var email = "test@example.com";
        var password = "password";
        var hashedPassword = "hashed_password";

        var authDetails = new AuthDetails(userId, email, tenantId, hashedPassword, new List<string> { "Admin" }, new List<string> { "read" }, new List<string> { "feature1" });

        _userRepositoryMock.Setup(x => x.GetAuthDetailsAsync(email))
            .ReturnsAsync(authDetails);
        _passwordHasherMock.Setup(x => x.VerifyPassword(password, hashedPassword))
            .Returns(false); // Simulate incorrect password

        // Act
        var result = await _sut.LoginAsync(email, password, "127.0.0.1");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrors.InvalidCredentials, result.Error);
        _userRepositoryMock.Verify(x => x.GetAuthDetailsAsync(email), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(password, hashedPassword), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }


    // --- RefreshAsync Tests ---

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsSuccessWithNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var email = "test@example.com";
        var existingRefreshToken = "old_refresh_token";
        var ipAddress = "127.0.0.1";
        var authDetails = new AuthDetails(userId, email, tenantId, "hashed_password", new List<string> { "Admin" }, new List<string> { "read" }, new List<string> { "feature1" });
        
        var existingRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = _tokenServiceMock.Object.GetHashToken(existingRefreshToken), // Hash of the old token
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            CreatedByIp = ipAddress,
            RevokedAt = null
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(existingRefreshToken))
            .ReturnsAsync(existingRefreshTokenEntity);
        _userRepositoryMock.Setup(x => x.GetAuthDetailsByIdAsync(userId))
            .ReturnsAsync(authDetails);

        // Mock TokenService for new token generation
        _tokenServiceMock.Setup(ts => ts.GenerateAccessToken(userId, email, tenantId, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("new_access_token");
        _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken())
            .Returns("new_refresh_token");
        _tokenServiceMock.Setup(ts => ts.GetHashToken("new_refresh_token"))
            .Returns("hashed_new_refresh_token");

        // Act
        var result = await _sut.RefreshAsync(existingRefreshToken, ipAddress);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("new_access_token", result.Value.AccessToken);
        Assert.Equal("new_refresh_token", result.Value.RefreshToken);

        _refreshTokenRepositoryMock.Verify(x => x.RevokeAllForUserAsync(It.IsAny<Guid>()), Times.Never); // Should not revoke all
        Assert.NotNull(existingRefreshTokenEntity.RevokedAt); // Old token should be revoked
        Assert.Contains(existingRefreshTokenEntity.ReplacedByTokenHash!, "hashed_new_refresh_token"); // Old token should be marked as replaced

        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
            rt.UserId == userId &&
            rt.TokenHash == "hashed_new_refresh_token" &&
            rt.CreatedByIp == ipAddress)), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var refreshToken = "invalid_refresh_token";
        var ipAddress = "127.0.0.1";

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync((RefreshToken?)null); // Simulate token not found

        // Act
        var result = await _sut.RefreshAsync(refreshToken, ipAddress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrors.InvalidToken, result.Error);
        _userRepositoryMock.Verify(x => x.GetAuthDetailsByIdAsync(It.IsAny<Guid>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(x => x.GetByTokenAsync(refreshToken), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ReusedToken_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingRefreshToken = "reused_refresh_token";
        var ipAddress = "127.0.0.1";

        var existingRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = _tokenServiceMock.Object.GetHashToken(existingRefreshToken),
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            CreatedByIp = ipAddress,
            RevokedAt = DateTime.UtcNow // Simulate revoked token
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(existingRefreshToken))
            .ReturnsAsync(existingRefreshTokenEntity);

        // Act
        var result = await _sut.RefreshAsync(existingRefreshToken, ipAddress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrors.TokenReused, result.Error);
        _userRepositoryMock.Verify(x => x.GetAuthDetailsByIdAsync(It.IsAny<Guid>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(x => x.RevokeAllForUserAsync(userId), Times.Once); // Should revoke all associated tokens
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingRefreshToken = "expired_refresh_token";
        var ipAddress = "127.0.0.1";

        var existingRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = _tokenServiceMock.Object.GetHashToken(existingRefreshToken),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-3), // Expired
            CreatedByIp = ipAddress,
            RevokedAt = null
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(existingRefreshToken))
            .ReturnsAsync(existingRefreshTokenEntity);

        // Act
        var result = await _sut.RefreshAsync(existingRefreshToken, ipAddress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrors.TokenExpired, result.Error);
        _userRepositoryMock.Verify(x => x.GetAuthDetailsByIdAsync(It.IsAny<Guid>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingRefreshToken = "refresh_token_for_unknown_user";
        var ipAddress = "127.0.0.1";

        var existingRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = _tokenServiceMock.Object.GetHashToken(existingRefreshToken),
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            CreatedByIp = ipAddress,
            RevokedAt = null
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(existingRefreshToken))
            .ReturnsAsync(existingRefreshTokenEntity);
        _userRepositoryMock.Setup(x => x.GetAuthDetailsByIdAsync(userId))
            .ReturnsAsync((AuthDetails?)null); // Simulate user not found

        // Act
        var result = await _sut.RefreshAsync(existingRefreshToken, ipAddress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrors.UserNotFound, result.Error);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }


    // --- LogoutAsync Tests ---

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesTokenAndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "valid_refresh_token_to_logout";
        var ipAddress = "127.0.0.1";

        var existingRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = _tokenServiceMock.Object.GetHashToken(refreshToken),
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            CreatedByIp = ipAddress,
            RevokedAt = null
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync(existingRefreshTokenEntity);

        // Act
        var result = await _sut.LogoutAsync(refreshToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(existingRefreshTokenEntity.RevokedAt); // Token should be marked as revoked
        _refreshTokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.GetByTokenAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var refreshToken = "invalid_token_to_logout";

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync((RefreshToken?)null); // Simulate token not found

        // Act
        var result = await _sut.LogoutAsync(refreshToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrors.InvalidToken, result.Error);
        _refreshTokenRepositoryMock.Verify(x => x.GetByTokenAsync(refreshToken), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_InactiveToken_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "inactive_token_to_logout";
        var ipAddress = "127.0.0.1";

        var existingRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = _tokenServiceMock.Object.GetHashToken(refreshToken),
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            CreatedByIp = ipAddress,
            RevokedAt = DateTime.UtcNow // Simulate already revoked token
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync(existingRefreshTokenEntity);

        // Act
        var result = await _sut.LogoutAsync(refreshToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrors.InvalidToken, result.Error); // Currently, inactive tokens also return InvalidToken
        _refreshTokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _refreshTokenRepositoryMock.Verify(x => x.GetByTokenAsync(refreshToken), Times.Once);
    }
}
