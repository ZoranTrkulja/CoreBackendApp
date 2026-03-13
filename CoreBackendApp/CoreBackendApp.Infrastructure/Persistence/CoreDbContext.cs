using CoreBackendApp.Application.Common.Interfaces;
using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Domain.Interfaces;
using CoreBackendApp.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Persistence
{
    public class CoreDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;

        public CoreDbContext(
            DbContextOptions<CoreDbContext> options,
            ITenantProvider tenantProvider) : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();

        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        public DbSet<Feature> Features => Set<Feature>();
        public DbSet<TenantFeature> TenantFeatures => Set<TenantFeature>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply Global Query Filter for Multi-Tenancy
            var currentTenantId = _tenantProvider.TenantId;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                    var tenantIdValue = System.Linq.Expressions.Expression.Constant(currentTenantId ?? Guid.Empty);
                    var comparison = System.Linq.Expressions.Expression.Equal(property, tenantIdValue);
                    var lambda = System.Linq.Expressions.Expression.Lambda(comparison, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
                entity.HasIndex(x => x.Email).IsUnique();

                entity.HasOne(x => x.Tenant)
                      .WithMany(t => t.Users)
                      .HasForeignKey(x => x.TenantId);
            });

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(256);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(256);
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Code).IsRequired().HasMaxLength(100);
                entity.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<Feature>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Key).IsRequired().HasMaxLength(256);
                entity.HasIndex(x => x.Key).IsUnique();
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(x => new { x.UserId, x.RoleId });

                entity.HasOne(x => x.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(x => x.UserId);

                entity.HasOne(x => x.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(x => x.RoleId);
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(x => new { x.RoleId, x.PermissionId });

                entity.HasOne(x => x.Role)
                      .WithMany(r => r.RolePermissions)
                      .HasForeignKey(x => x.RoleId);

                entity.HasOne(x => x.Permission)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(x => x.PermissionId);
            });

            modelBuilder.Entity<TenantFeature>(entity =>
            {
                entity.HasKey(x => new { x.TenantId, x.FeatureId });

                entity.HasOne(x => x.Tenant)
                      .WithMany(t => t.TenantFeatures)
                      .HasForeignKey(x => x.TenantId);

                entity.HasOne(x => x.Feature)
                      .WithMany(f => f.TenantFeatures)
                      .HasForeignKey(x => x.FeatureId);
            });

            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        }
    }
}
