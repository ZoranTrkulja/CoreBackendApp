using CoreBackendApp.Application.Auth;
using CoreBackendApp.Application.Interface;
using CoreBackendApp.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBackendApp.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IFeatureService, FeatureService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
