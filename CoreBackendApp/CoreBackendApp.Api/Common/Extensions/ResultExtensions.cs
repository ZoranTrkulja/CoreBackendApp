using CoreBackendApp.Application.Common.Models;

namespace CoreBackendApp.Api.Common.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Can't convert success result to problem details");
        }

        return Results.Problem(
            title: GetTitle(result.Error.Type),
            detail: result.Error.Message,
            statusCode: GetStatusCode(result.Error),
            extensions: new Dictionary<string, object?>
            {
                { "errors", new[] { result.Error.Code } }
            });
    }

    private static string GetTitle(ErrorType type) =>
        type switch
        {
            ErrorType.NotFound => "Not Found",
            ErrorType.Validation => "Bad Request",
            ErrorType.Conflict => "Conflict",
            _ => "Server Error"
        };

    private static int GetStatusCode(Error error)
    {
        if (error.Code.StartsWith("Auth."))
        {
            return StatusCodes.Status401Unauthorized;
        }

        return error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
