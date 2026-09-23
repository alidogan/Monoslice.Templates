using Microsoft.AspNetCore.Http;
using Shared.Contracts.Results;

namespace Shared.Results;

/// <summary>Maps <see cref="Result"/>s to HTTP responses; failures become RFC 9457 problem details.</summary>
public static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this Result result, Func<IResult> onSuccess) =>
        result.IsSuccess ? onSuccess() : result.Error.ToProblem();

    public static IResult ToHttpResult<TValue>(this Result<TValue> result, Func<TValue, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : result.Error.ToProblem();

    public static IResult ToHttpResult<TValue>(this Result<TValue> result) =>
        result.ToHttpResult(value => TypedResults.Ok(value));

    public static IResult ToProblem(this Error error)
    {
        var (statusCode, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorType.PreconditionFailed => (StatusCodes.Status412PreconditionFailed, "Precondition Failed"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error"),
        };

        return TypedResults.Problem(
            statusCode: statusCode,
            title: title,
            detail: error.Description,
            extensions: new Dictionary<string, object?>(StringComparer.Ordinal) { ["code"] = error.Code });
    }
}
