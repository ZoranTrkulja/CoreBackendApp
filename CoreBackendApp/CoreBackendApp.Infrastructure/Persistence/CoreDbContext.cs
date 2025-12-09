using CoreBackendApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Persistence
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();

        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        public DbSet<Feature> Features => Set<Feature>();
        public DbSet<TenantFeature> TenantFeatures => Set<TenantFeature>();

        //public override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<User>(entity => 
        //    {
        //        entity.HasKey(x => x.Id);
        //        entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
        //        entity.HasIndex(x => x.Email).IsUnique();

        //        entity.HasOne(x => x.Tenant)
        //              .WithMany(t => t.Users)
        //              .HasForeignKey(x => x.TenantId);
        //    });

        //    modelBuilder.Entity<Tenant>(entity =>
        //    {
        //        entity.HasKey(x => x.Id);
        //        entity.Property(x => x.Name).IsRequired().HasMaxLength(256);
        //    });

        //    modelBuilder.Entity<Role>(entity =>
        //    {
        //        entity.HasKey(x => x.Id);
        //        entity.Property(x => x.Name).IsRequired().HasMaxLength(256);
        //    });

        //    modelBuilder.Entity<Permission>(entity =>
        //    {
        //        entity.HasKey(x => x.Id);
        //        entity.Property(x => x.Code).IsRequired().HasMaxLength(100);
        //        entity.HasIndex(x => x.Code).IsUnique();
        //    });

        //    modelBuilder.Entity<Feature>(entity =>
        //    {
        //        entity.HasKey(x => x.Id);
        //        entity.Property(x => x.Key).IsRequired().HasMaxLength(256);
        //        entity.HasIndex(x => x.Key).IsUnique();
        //    });

            
        //}

    }
}
