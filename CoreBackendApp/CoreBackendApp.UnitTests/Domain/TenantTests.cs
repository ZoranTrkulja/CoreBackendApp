using CoreBackendApp.Domain.Entities;
using Xunit;

namespace CoreBackendApp.UnitTests.Domain;

public class TenantTests
{
    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var name = "Test Tenant";

        // Act
        var tenant = Tenant.Create(name);

        // Assert
        Assert.Equal(name, tenant.Name);
        Assert.NotEqual(Guid.Empty, tenant.Id);
        Assert.True(tenant.CreatedAt > DateTime.MinValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Tenant.Create(name!));
    }
}
