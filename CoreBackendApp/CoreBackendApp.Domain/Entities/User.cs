using CoreBackendApp.Domain.Common;

namespace CoreBackendApp.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; private set; } = default;
        public string PasswordHash { get; private set; } = default;

        public bool IsEmailConfirmed { get; private set; } = false;

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = default!;

        public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

        public User()
        {

        }

        public User(string email, string passwordHash, Guid tenantId)
        {
            Email = email;
            PasswordHash = passwordHash;
            TenantId = tenantId;
        }

        public void ConfirmEmail()
        {
            IsEmailConfirmed = true;
            MarkAsUpdated();
        }
    }
}
