using CoreBackendApp.Application.Common.Models;

namespace CoreBackendApp.Application.Users;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "User not found.");
    public static readonly Error InvalidTenant = new("User.InvalidTenant", "Invalid Tenant.");
    public static readonly Error UserAlreadyHasRole = new("User.UserAlreadyHasRole", "User already has this role.");
    public static readonly Error RoleNotFound = new("User.RoleNotFound", "Role not found.");
}
