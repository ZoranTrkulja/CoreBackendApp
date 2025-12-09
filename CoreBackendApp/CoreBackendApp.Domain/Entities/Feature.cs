using CoreBackendApp.Domain.Common;

namespace CoreBackendApp.Domain.Entities
{
    public class Feature : BaseEntity
    {
        public string Key { get; private set; } = default!;
        public string Name { get; private set; } = default!;

        public ICollection<TenantFeature> TenantFeatures { get; private set; } = new List<TenantFeature>();

        public Feature()
        {
        }   
        public Feature(string key, string name)
        {
            Key = key;
            Name = name;
        }
    }
}
