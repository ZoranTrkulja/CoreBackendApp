using CoreBackendApp.Domain.Entities;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CoreBackendApp.Api.Common.Validation;

namespace CoreBackendApp.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("users").WithTags("Users");

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
            var tenantExists = await coreDbContext.Tenants.AnyAsync(t => t.Id == request.TenantId);
            if (!tenantExists)
                return Results.BadRequest("Invalid Tenant");

            var user = User.Create(
                request.Email, 
                BCrypt.Net.BCrypt.HashPassword(request.Password), 
                request.TenantId);

            coreDbContext.Users.Add(user);
            await coreDbContext.SaveChangesAsync();

            return Results.Created($"/v1/users/{user.Id}", user.Id);
        })
        .AddEndpointFilter<ValidationFilter<CreateUserRequest>>()
        .RequireAuthorization("RequireUsersManagePermission");


        group.MapPost("/{id:guid}/assign-role/{roleId:guid}", async (Guid id, Guid roleId, CoreDbContext coreDbContext) =>
        {
            var user = await coreDbContext.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return Results.NotFound("User not found");

            var roleExists = await coreDbContext.Roles.AnyAsync(r => r.Id == roleId);
            if (!roleExists)
                return Results.NotFound("Role not found");

            if (user.UserRoles.Any(ur => ur.RoleId == roleId))
                return Results.BadRequest("User already has this role");

            // Use the rich domain method
            user.AssignRole(roleId);

            await coreDbContext.SaveChangesAsync();
            return Results.Ok("Role assigned to user successfully");
        })
       .RequireAuthorization("RequireUsersManagePermission");

        return endpointRouteBuilder;
    }
}

public record CreateUserRequest(string Email, string Password, Guid TenantId);
