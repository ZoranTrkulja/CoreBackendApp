using CoreBackendApp.Domain.Common;

namespace CoreBackendApp.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; protected set; } = default!;

        public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

        public Role()
        {
        }

        public Role(string name)
        {
            Name = name;
        }
    }
}
