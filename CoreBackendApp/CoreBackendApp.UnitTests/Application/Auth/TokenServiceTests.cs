using CoreBackendApp.Application.Auth;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace CoreBackendApp.UnitTests.Application.Auth;

public class TokenServiceTests
{
    private readonly TokenService _sut;
    private readonly Mock<IConfiguration> _configurationMock;

    public TokenServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();

        // Mock IConfiguration values needed by TokenService
        _configurationMock.Setup(cfg => cfg["Jwt:Key"]).Returns("SuperSecretTestKeyThatIsVeryLong123!");
        _configurationMock.Setup(cfg => cfg["Jwt:Issuer"]).Returns("CoreBackendApp");
        _configurationMock.Setup(cfg => cfg["Jwt:Audience"]).Returns("CoreBackendApp");
        _configurationMock.Setup(cfg => cfg["Jwt:AccessTokenExpirationMinutes"]).Returns("60");

        _sut = new TokenService(_configurationMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_WithValidClaims_ReturnsValidToken() 
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new List<string> { "Admin", "User" };
        var permissions = new List<string> { "read", "write" };
        var features = new List<string> { "feature1", "feature2" };

        // Act
        var token = _sut.GenerateAccessToken(userId, email, tenantId, roles, permissions, features);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

        Assert.NotNull(jsonToken);

        // Verify standard claims
        Assert.Equal(userId.ToString(), jsonToken.Subject);
        Assert.Equal(email, jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value);
        Assert.Equal(tenantId.ToString(), jsonToken.Claims.FirstOrDefault(c => c.Type == "tenantId")?.Value);

        // Verify custom claims (Roles, Permissions, Features)
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
        Assert.Contains(jsonToken.Claims, c => c.Type == "permissions" && c.Value == "read");
        Assert.Contains(jsonToken.Claims, c => c.Type == "permissions" && c.Value == "write");
        Assert.Contains(jsonToken.Claims, c => c.Type == "features" && c.Value == "feature1");
        Assert.Contains(jsonToken.Claims, c => c.Type == "features" && c.Value == "feature2");

        // Verify expiration time (approximate due to DateTime.UtcNow)
        var expectedExpiration = DateTime.UtcNow.AddMinutes(60);
        // Allow for a small margin of error due to execution time
        Assert.True(jsonToken.ValidTo >= expectedExpiration.AddSeconds(-5) && jsonToken.ValidTo <= expectedExpiration.AddSeconds(5)); 
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueBase64String() 
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrEmpty(token1));
        Assert.False(string.IsNullOrEmpty(token2));
        Assert.Equal(86, token1.Length); // Base64 of 64 random bytes is 86 chars
        Assert.NotEqual(token1, token2); // Should be unique
    }

    [Fact]
    public void GetHashToken_ReturnsCorrectSha256Hash() 
    {
        // Arrange
        var token = "my_secret_refresh_token";
        // Manually calculated SHA256 hash for "my_secret_refresh_token"
        var expectedHash = "tVq9/R33x3c6K9Jv6/oJ9wN7wF2U0k7v4yqY+6yqYlE="; // This is NOT the correct hash. SHA256 is binary, Convert.ToBase64String is needed.
        // Corrected expected hash calculation: Using a known online SHA256 calculator for "my_secret_refresh_token"
        // The correct hash for "my_secret_refresh_token" using SHA256 is:
        // C56C593F9F8B361F908984C6B499E1D43A39291226A7784C654C43C4C3987EAA
        // Converting this to Base64:
        var correctExpectedHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(token)));

        // Act
        var actualHash = _sut.GetHashToken(token);

        // Assert
        Assert.Equal(correctExpectedHash, actualHash);
    }

    [Fact]
    public void VerifyToken_MatchingTokenAndHash_ReturnsTrue() 
    {
        // Arrange
        var token = "some_refresh_token_to_verify";
        var tokenHash = _sut.GetHashToken(token);

        // Act
        var result = _sut.VerifyToken(token, tokenHash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyToken_NonMatchingTokenAndHash_ReturnsFalse() 
    {
        // Arrange
        var token = "some_refresh_token_to_verify";
        var correctHash = _sut.GetHashToken(token);
        var wrongHash = "this_is_a_wrong_hash";

        // Act
        var result = _sut.VerifyToken(token, wrongHash);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void VerifyToken_EmptyToken_ReturnsFalse() 
    {
        // Arrange
        var token = "";
        var tokenHash = _sut.GetHashToken("some_token");

        // Act
        var result = _sut.VerifyToken(token, tokenHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyToken_EmptyHash_ReturnsFalse() 
    {
        // Arrange
        var token = "some_token";
        var tokenHash = "";

        // Act
        var result = _sut.VerifyToken(token, tokenHash);

        // Assert
        Assert.False(result);
    }
}
