namespace CoreBackendApp.Application.Users;

public record UserResponse(Guid Id, string Email, Guid TenantId, IEnumerable<string> Roles);
