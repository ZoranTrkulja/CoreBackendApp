namespace CoreBackendApp.Application.Users;

public record CreateUserRequest(string Email, string Password, Guid TenantId);
