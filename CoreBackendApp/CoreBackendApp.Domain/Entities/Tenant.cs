using CoreBackendApp.Domain.Common;

namespace CoreBackendApp.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = default!;

    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<TenantFeature> TenantFeatures { get; private set; } = new List<TenantFeature>();

    private Tenant() { }

    public static Tenant Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.");

        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }
}
