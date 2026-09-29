using CineVault.Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return typeof(T) == typeof(None)
                ? new NoContentResult()
                : new OkObjectResult(result.Value);
        }

        var error = result.Error!;
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(new { error.Code, error.Message }) { StatusCode = statusCode };
    }
}
