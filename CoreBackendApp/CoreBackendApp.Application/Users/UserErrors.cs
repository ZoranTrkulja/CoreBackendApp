using CoreBackendApp.Application.Common.Models;

namespace CoreBackendApp.Application.Users;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound("User.NotFound", "User not found.");
    public static readonly Error InvalidTenant = Error.Validation("User.InvalidTenant", "Invalid Tenant.");
    public static readonly Error EmailAlreadyExists = Error.Conflict("User.EmailAlreadyExists", "Email already exists.");
    public static readonly Error UserAlreadyHasRole = Error.Conflict("User.UserAlreadyHasRole", "User already has this role.");
    public static readonly Error RoleNotFound = Error.NotFound("User.RoleNotFound", "Role not found.");
}
