using CoreBackendApp.Application.Auth;
using FluentValidation.TestHelper;
using Xunit;

namespace CoreBackendApp.UnitTests.Application.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldHaveError(string email)
    {
        // Arrange
        var request = new LoginRequest(email, "password123");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        var request = new LoginRequest("invalid-email", "password123");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyPassword_ShouldHaveError(string password)
    {
        // Arrange
        var request = new LoginRequest("test@example.com", password);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
