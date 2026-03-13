using CoreBackendApp.Domain.Entities;
using Xunit;

namespace CoreBackendApp.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashed_password";
        var tenantId = Guid.NewGuid();

        // Act
        var user = User.Create(email, passwordHash, tenantId);

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(tenantId, user.TenantId);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.True(user.CreatedAt > DateTime.MinValue);
        Assert.False(user.IsEmailConfirmed);
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var email = "";
        var passwordHash = "hashed_password";
        var tenantId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => User.Create(email, passwordHash, tenantId));
    }

    [Fact]
    public void AssignRole_ShouldAddRoleToCollection()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", Guid.NewGuid());
        var roleId = Guid.NewGuid();

        // Act
        user.AssignRole(roleId);

        // Assert
        Assert.Single(user.UserRoles);
        Assert.Equal(roleId, user.UserRoles.First().RoleId);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void AssignRole_DuplicateRole_ShouldNotAddTwice()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", Guid.NewGuid());
        var roleId = Guid.NewGuid();

        // Act
        user.AssignRole(roleId);
        user.AssignRole(roleId);

        // Assert
        Assert.Single(user.UserRoles);
    }

    [Fact]
    public void ConfirmEmail_ShouldSetIsEmailConfirmedToTrue()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", Guid.NewGuid());

        // Act
        user.ConfirmEmail();

        // Assert
        Assert.True(user.IsEmailConfirmed);
        Assert.NotNull(user.UpdatedAt);
    }
}
