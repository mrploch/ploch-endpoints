using Ardalis.Result;

namespace Ploch.Common.Ardalis.Result;

public static class ResultStatusExtensions
{
    public static int ToHttpStatusCode(this ResultStatus resultStatus) =>
        resultStatus switch
        {
            ResultStatus.Ok => 200,
            ResultStatus.Created => 201,
            ResultStatus.Conflict => 409,
            ResultStatus.Unavailable => 503,
            ResultStatus.NoContent => 204,
            ResultStatus.NotFound => 404,
            ResultStatus.Invalid => 400,
            ResultStatus.Unauthorized => 401,
            ResultStatus.Forbidden => 403,
            ResultStatus.Error => 500,
            _ => 500
        };
}
