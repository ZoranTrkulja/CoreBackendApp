using CoreBackendApp.Domain.Common;

namespace CoreBackendApp.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; private set; } = default!;

        public ICollection<User> Users { get; private set; } = new List<User>();
        public ICollection<TenantFeature> TenantFeatures { get; private set; } = new List<TenantFeature>();

        public Tenant()
        {
        }

        public Tenant(string name)
        {
            Name = name;
        }
    }
}
