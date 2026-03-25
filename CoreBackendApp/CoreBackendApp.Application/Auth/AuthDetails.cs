namespace CoreBackendApp.Application.Auth;

public record AuthDetails(
    Guid UserId,
    string Email,
    string PasswordHash,
    Guid TenantId,
    IEnumerable<string> Roles,
    IEnumerable<string> Permissions,
    IEnumerable<string> Features
);
