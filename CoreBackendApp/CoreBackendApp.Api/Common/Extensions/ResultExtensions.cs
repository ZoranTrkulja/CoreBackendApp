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

        return Results.Json(result.Error, statusCode: GetStatusCode(result.Error.Code));
    }

    private static int GetStatusCode(string errorCode)
    {
        if (errorCode.EndsWith(".NotFound"))
        {
            return StatusCodes.Status404NotFound;
        }

        if (errorCode.EndsWith(".Unauthorized") || errorCode.StartsWith("Auth."))
        {
            return StatusCodes.Status401Unauthorized;
        }

        return StatusCodes.Status400BadRequest;
    }
}
