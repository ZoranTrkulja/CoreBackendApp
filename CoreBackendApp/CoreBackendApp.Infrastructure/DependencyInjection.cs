using CoreBackendApp.Application.Common.Interfaces;
using CoreBackendApp.Application.Interface;
using CoreBackendApp.Infrastructure.Identity;
using CoreBackendApp.Infrastructure.Persistence;
using CoreBackendApp.Infrastructure.Repositories;
using CoreBackendApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBackendApp.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        bool isTesting)
    {
        if (isTesting)
        {
            services.AddDbContext<CoreDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });
        }
        else
        {
            services.AddDbContext<CoreDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? 
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
            });
        }

        services.AddScoped<ITenantProvider, TenantProvider>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddHealthChecks().AddDbContextCheck<CoreDbContext>();

        return services;
    }
}
