using CoreBackendApp.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CoreBackendApp.UnitTests.Infrastructure.Services;

public class TenantProviderTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly TenantProvider _tenantProvider;

    public TenantProviderTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _tenantProvider = new TenantProvider(_mockHttpContextAccessor.Object);
    }

    [Fact]
    public void TenantId_ShouldReturnGuid_WhenClaimExistsAndIsValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var claims = new List<Claim> { new("tenantId", tenantId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _tenantProvider.TenantId;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result);
    }

    [Fact]
    public void TenantId_ShouldReturnNull_WhenClaimIsMissing()
    {
        // Arrange
        var claims = new List<Claim>(); // No tenantId claim
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _tenantProvider.TenantId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TenantId_ShouldReturnNull_WhenClaimIsInvalidGuid()
    {
        // Arrange
        var claims = new List<Claim> { new("tenantId", "invalid-guid") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _tenantProvider.TenantId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TenantId_ShouldReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _tenantProvider.TenantId;

        // Assert
        Assert.Null(result);
    }
}
