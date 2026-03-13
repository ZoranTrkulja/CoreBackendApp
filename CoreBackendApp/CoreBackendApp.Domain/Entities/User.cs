using CoreBackendApp.Domain.Common;

namespace CoreBackendApp.Domain.Entities;

public class User : BaseEntity
{
    // Use private setters to protect the domain state
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsEmailConfirmed { get; private set; } = false;
    public Guid TenantId { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = default!;
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    // Required for EF Core
    private User() { }

    // Static Factory Method for explicit creation
    public static User Create(string email, string passwordHash, Guid tenantId)
    {
        // Add domain validation here if needed (e.g., email format)
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty.");
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID must be provided.");

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash)) 
            throw new ArgumentException("New password hash cannot be empty.");

        PasswordHash = newPasswordHash;
        MarkAsUpdated();
    }

    public void ConfirmEmail()
    {
        if (IsEmailConfirmed) return;

        IsEmailConfirmed = true;
        MarkAsUpdated();
    }

    public void AssignRole(Guid roleId)
    {
        if (UserRoles.Any(ur => ur.RoleId == roleId)) return;

        UserRoles.Add(new UserRole
        {
            UserId = Id,
            RoleId = roleId
        });
        
        MarkAsUpdated();
    }
}
