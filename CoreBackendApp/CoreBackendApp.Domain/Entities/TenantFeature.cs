namespace CoreBackendApp.Domain.Entities
{
    public class TenantFeature
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = default!;

        public Guid FeatureId { get; set; }
        public Feature Feature { get; set; } = default!;
    }
}
