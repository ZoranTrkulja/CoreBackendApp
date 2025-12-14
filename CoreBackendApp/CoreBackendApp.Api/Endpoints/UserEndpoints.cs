using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBackendApp.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            var group = endpointRouteBuilder.MapGroup("/api/users").WithTags("Users");

            group.MapGet("/", async (CoreDbContext coreDbContext) =>
            {
                var users = await coreDbContext.Users
                    .Select(u => new { 
                        u.Id, 
                        u.Email, 
                        u.TenantId,
                        Roles = u.UserRoles.Select(ur => ur.Role.Name)
                    })
                    .ToListAsync();

                return Results.Ok(users);
            });

            group.MapGet("/{id:guid}", async (Guid id, CoreDbContext coreDbContext) =>
            {
                var user = await coreDbContext.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.TenantId,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name)
                })
                .FirstOrDefaultAsync();

                return Results.Ok(user);

            });

            group.MapPost("/", async (CreateUserRequest request, CoreDbContext coreDbContext) =>
            {
                var tenant = await coreDbContext.Tenants.FindAsync(request.TenantId);
                if (tenant == null)
                    return Results.BadRequest("Invalid Tenant");

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    TenantId = request.TenantId
                };

                coreDbContext.Users.Add(user);
                await coreDbContext.SaveChangesAsync();

                return Results.Created($"/api/users/{user.Id}", user.Id);
            })
            .RequireAuthorization("RequireUsersManagePermission");


            group.MapPost("/{id:guid}/assign-role/{roleId:guid}", async (Guid id, Guid roleId, CoreDbContext coreDbContext) =>
            {
                var user = await coreDbContext.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return Results.NotFound("User not found");
                var role = await coreDbContext.Roles.FindAsync(roleId);
                if (role == null)
                    return Results.NotFound("Role not found");
                if (user.UserRoles.Any(ur => ur.RoleId == roleId))
                    return Results.BadRequest("User already has this role");
                user.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
                await coreDbContext.SaveChangesAsync();
                return Results.Ok("Role assigned to user successfully");
            })
           .RequireAuthorization("RequireUsersManagePermission");

            return endpointRouteBuilder;
        }
    }
}

public record CreateUserRequest(string Email, string Password, Guid TenantId);
