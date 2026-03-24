using CoreBackendApp.Application.Users;
using CoreBackendApp.Domain.Entities;
using Mapster;

namespace CoreBackendApp.Api.Common;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<User, UserResponse>.NewConfig()
            .Map(dest => dest.Roles, src => src.UserRoles.Select(ur => ur.Role.Name));
    }
}
