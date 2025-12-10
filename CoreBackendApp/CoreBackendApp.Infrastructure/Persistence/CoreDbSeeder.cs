using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Infrastructure.Persistence
{
    public class CoreDbSeeder
    {
        public static async Task SeedAsync(CoreDbContext context)
        {
            if (!context.Tenants.Any())
            {
                var tenant = new Tenant("System");

                var adminRole = new Role("Admin");
                var userRole = new Role("User");

                var axiomFeature = new Feature("AxiomWork", "AxiomWork");
                var structinoFeature = new Feature("Structino", "Structino");

                var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

                var adminUser = new User("admin@core.local", passwordHash, tenant.Id);

                context.Tenants.Add(tenant);
                context.Roles.AddRange(adminRole, userRole);
                context.Features.AddRange(axiomFeature, structinoFeature); 
                context.Users.Add(adminUser);

                await context.SaveChangesAsync();

                context.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });

                context.TenantFeatures.AddRange(new List<TenantFeature>()
                {
                    new() {
                        TenantId = tenant.Id,
                        FeatureId = axiomFeature.Id
                    },
                    new() {
                        TenantId = tenant.Id,
                        FeatureId = structinoFeature.Id
                    }
                });


                await context.SaveChangesAsync();
            }
        }
    }
}
