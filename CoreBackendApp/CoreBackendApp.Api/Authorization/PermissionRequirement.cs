using Microsoft.AspNetCore.Authorization;

namespace CoreBackendApp.Api.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
