using CoreBackendApp.Domain.Common;

namespace CoreBackendApp.Domain.Entities
{
    public class Permission : BaseEntity
    {
        public string Code { get; private set; } = default!;
        public string Description { get; private set; } = default!;

        public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

        public Permission()
        {
            
        }

        public Permission(string code, string description)
        {
            Code = code;
            Description = description;
        }
    }
}
