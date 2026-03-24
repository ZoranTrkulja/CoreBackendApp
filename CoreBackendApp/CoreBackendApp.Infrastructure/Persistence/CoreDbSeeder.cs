using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Infrastructure.Persistence
{
    public class CoreDbSeeder
    {
        public static async Task SeedAsync(CoreDbContext context)
        {
            if (!context.Tenants.Any())
            {
                var permissions = new[]
                {
                    new Permission{ Id = Guid.NewGuid(), Code = "user.read", Description = "Read users", CreatedAt = DateTime.UtcNow },
                    new Permission{ Id = Guid.NewGuid(), Code = "user.create", Description = "Create users", CreatedAt = DateTime.UtcNow },
                    new Permission{ Id = Guid.NewGuid(), Code = "user.manage", Description = "Manage all user data", CreatedAt = DateTime.UtcNow },

                    new Permission { Id = Guid.NewGuid(), Code = "roles.read", Description = "Read roles", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid(), Code = "roles.manage", Description = "Manage roles", CreatedAt = DateTime.UtcNow },

                    new Permission { Id = Guid.NewGuid(), Code = "permissions.read", Description = "Read permissions", CreatedAt = DateTime.UtcNow },

                    new Permission { Id = Guid.NewGuid(), Code = "features.read", Description = "Read features", CreatedAt = DateTime.UtcNow },

                    new Permission { Id = Guid.NewGuid(), Code = "tenants.read", Description = "Read tenants", CreatedAt = DateTime.UtcNow },
                    new Permission { Id = Guid.NewGuid(), Code = "tenants.manage", Description = "Manage tenants", CreatedAt = DateTime.UtcNow }
                };


                foreach (var permission in permissions)
                {
                    if (!context.Permissions.Any(x => x.Code == permission.Code))
                        context.Permissions.Add(permission);
                }

                var tenant = Tenant.Create("System");

                var adminRole = Role.Create("Admin");
                var userRole = Role.Create("User");

                var axiomFeature = Feature.Create("AxiomWork", "AxiomWork");
                var structinoFeature = Feature.Create("Structino", "Structino");

                var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

                var adminUser = User.Create("admin@core.local", passwordHash, tenant.Id);

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

                foreach (var permission in permissions)
                {
                    if(!context.RolePermissions.Any(rp => rp.RoleId == adminRole.Id && rp.PermissionId == permission.Id))
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = adminRole.Id,
                            PermissionId = permission.Id
                        });
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
