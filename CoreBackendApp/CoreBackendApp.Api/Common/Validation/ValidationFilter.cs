using FluentValidation;

namespace CoreBackendApp.Api.Common.Validation
{
    public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var argument = context.Arguments.FirstOrDefault(a => a is T) as T;

            if (argument == null)
            {
                return Results.BadRequest("Invalid request body.");
            }

            var validationResult = await validator.ValidateAsync(argument);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            return await next(context);
        }
    }
}
