using CoreBackendApp.Application.Auth;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CoreBackendApp.IntegrationTests.Auth;

public class LoginTests : BaseIntegrationTest
{
    public LoginTests(WebApplicationFactory<Api.Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Debug_CheckUserExists()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == "admin@core.local");
        Assert.NotNull(user);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkAndToken()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@core.local", "Admin123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.AccessToken);
        Assert.NotEmpty(loginResponse.RefreshToken);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@core.local", "WrongPassword");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        // Note: Our current implementation returns 401 via Exception Middleware or AuthService
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
