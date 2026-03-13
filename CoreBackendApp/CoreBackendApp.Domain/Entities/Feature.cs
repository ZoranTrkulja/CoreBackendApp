using CoreBackendApp.Domain.Common;

namespace CoreBackendApp.Domain.Entities;

public class Feature : BaseEntity
{
    public string Key { get; private set; } = default!;
    public string Name { get; private set; } = default!;

    public ICollection<TenantFeature> TenantFeatures { get; private set; } = new List<TenantFeature>();

    private Feature() { }

    public static Feature Create(string key, string name)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be empty.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.");

        return new Feature
        {
            Id = Guid.NewGuid(),
            Key = key,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }
}
