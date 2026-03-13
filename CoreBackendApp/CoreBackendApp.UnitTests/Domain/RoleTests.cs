using CoreBackendApp.Domain.Entities;
using Xunit;

namespace CoreBackendApp.UnitTests.Domain;

public class RoleTests
{
    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var name = "Admin";

        // Act
        var role = Role.Create(name);

        // Assert
        Assert.Equal(name, role.Name);
        Assert.NotEqual(Guid.Empty, role.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Role.Create(name!));
    }
}
