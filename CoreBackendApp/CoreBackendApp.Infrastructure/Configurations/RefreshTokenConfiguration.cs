using CoreBackendApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Infrastructure.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(512);

            builder.HasIndex(x => x.TokenHash)
                .IsUnique();

            builder.Property(x => x.CreatedByIp)
                .HasMaxLength(100);
        }
    }
}
