using CoreBackendApp.Domain.Common;

namespace CoreBackendApp.Domain.Entities
{
    public class Permission : BaseEntity
    {
        public string Code { get; set; } = default!;
        public string Description { get;  set; } = default!;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

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
