using CoreBackendApp.Application.Common.Interfaces;
using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Interface;
using CoreBackendApp.Application.Services;
using CoreBackendApp.Application.Users;
using CoreBackendApp.Domain.Entities;
using Moq;
using Xunit;

namespace CoreBackendApp.UnitTests.Application.Users;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _sut = new UserService(
            _userRepositoryMock.Object,
            _tenantRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedList()
    {
        // Arrange
        var paginationParams = new PaginationParams();
        var expectedPagedList = new PagedList<UserResponse>(new List<UserResponse>(), 0, 1, 10);
        _userRepositoryMock.Setup(x => x.GetAllWithRolesAsync(paginationParams))
            .ReturnsAsync(expectedPagedList);

        // Act
        var result = await _sut.GetAllAsync(paginationParams);

        // Assert
        Assert.Equal(expectedPagedList, result);
    }

    [Fact]
    public async Task GetByIdAsync_UserExists_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userResponse = new UserResponse(userId, "test@example.com", Guid.NewGuid(), new List<string>());
        _userRepositoryMock.Setup(x => x.GetByIdWithRolesAsync(userId))
            .ReturnsAsync(userResponse);

        // Act
        var result = await _sut.GetByIdAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(userResponse, result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_UserDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdWithRolesAsync(userId))
            .ReturnsAsync((UserResponse?)null);

        // Act
        var result = await _sut.GetByIdAsync(userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CreateUserRequest("test@example.com", "password", Guid.NewGuid());
        _tenantRepositoryMock.Setup(x => x.ExistsAsync(request.TenantId)).ReturnsAsync(true);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(x => x.HashPassword(request.Password)).Returns("hashed_password");

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidTenant_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateUserRequest("test@example.com", "password", Guid.NewGuid());
        _tenantRepositoryMock.Setup(x => x.ExistsAsync(request.TenantId)).ReturnsAsync(false);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.InvalidTenant, result.Error);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateUserRequest("test@example.com", "password", Guid.NewGuid());
        _tenantRepositoryMock.Setup(x => x.ExistsAsync(request.TenantId)).ReturnsAsync(true);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(User.Create(request.Email, "hash", request.TenantId));

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.EmailAlreadyExists, result.Error);
    }

    [Fact]
    public async Task AssignRoleAsync_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = User.Create("test@example.com", "hash", Guid.NewGuid());
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _roleRepositoryMock.Setup(x => x.ExistsAsync(roleId)).ReturnsAsync(true);

        // Act
        var result = await _sut.AssignRoleAsync(userId, roleId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(user.UserRoles);
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task AssignRoleAsync_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.AssignRoleAsync(userId, Guid.NewGuid());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task AssignRoleAsync_RoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = User.Create("test@example.com", "hash", Guid.NewGuid());
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _roleRepositoryMock.Setup(x => x.ExistsAsync(roleId)).ReturnsAsync(false);

        // Act
        var result = await _sut.AssignRoleAsync(userId, roleId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.RoleNotFound, result.Error);
    }

    [Fact]
    public async Task AssignRoleAsync_UserAlreadyHasRole_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = User.Create("test@example.com", "hash", Guid.NewGuid());
        user.AssignRole(roleId);
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _roleRepositoryMock.Setup(x => x.ExistsAsync(roleId)).ReturnsAsync(true);

        // Act
        var result = await _sut.AssignRoleAsync(userId, roleId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.UserAlreadyHasRole, result.Error);
    }
}
