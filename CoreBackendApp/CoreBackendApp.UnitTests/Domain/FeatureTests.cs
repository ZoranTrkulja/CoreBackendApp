using CoreBackendApp.Domain.Entities;
using Xunit;

namespace CoreBackendApp.UnitTests.Domain;

public class FeatureTests
{
    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var key = "users";
        var name = "User Management";

        // Act
        var feature = Feature.Create(key, name);

        // Assert
        Assert.Equal(key, feature.Key);
        Assert.Equal(name, feature.Name);
        Assert.NotEqual(Guid.Empty, feature.Id);
    }

    [Theory]
    [InlineData("", "Valid Name")]
    [InlineData("Valid Key", "")]
    [InlineData(" ", "Valid Name")]
    [InlineData("Valid Key", " ")]
    [InlineData(null, "Valid Name")]
    [InlineData("Valid Key", null)]
    public void Create_WithInvalidArguments_ShouldThrowArgumentException(string? key, string? name)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Feature.Create(key!, name!));
    }
}
