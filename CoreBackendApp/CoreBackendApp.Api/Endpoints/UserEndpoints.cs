using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Users;
using CoreBackendApp.Api.Common.Extensions;
using CoreBackendApp.Api.Common.Validation;
using CoreBackendApp.Application.Interface;

namespace CoreBackendApp.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("users").WithTags("Users");

        group.MapGet("/", async ([AsParameters] PaginationParams paginationParams, IUserService userService) =>
        {
            var users = await userService.GetAllAsync(paginationParams);
            return Results.Ok(users);
        })
        .RequireAuthorization("user.read");

        group.MapGet("/{id:guid}", async (Guid id, IUserService userService) =>
        {
            var result = await userService.GetByIdAsync(id);

            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        })
        .RequireAuthorization("user.read");

        group.MapPost("/", async (CreateUserRequest request, IUserService userService) =>
        {
            var result = await userService.CreateAsync(request);

            return result.IsSuccess 
                ? Results.Created($"/v1/users/{result.Value}", result.Value) 
                : result.ToProblemDetails();
        })
        .AddEndpointFilter<ValidationFilter<CreateUserRequest>>()
        .RequireAuthorization("user.create");


        group.MapPost("/{id:guid}/assign-role/{roleId:guid}", async (Guid id, Guid roleId, IUserService userService) =>
        {
            var result = await userService.AssignRoleAsync(id, roleId);
            
            return result.IsSuccess 
                ? Results.Ok("Role assigned to user successfully") 
                : result.ToProblemDetails();
        })
       .RequireAuthorization("user.manage");

        group.MapDelete("/{id:guid}", async (Guid id, IUserService userService) =>
        {
            var result = await userService.DeleteAsync(id);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblemDetails();
        })
        .RequireAuthorization("user.manage");

        return endpointRouteBuilder;
    }
}
