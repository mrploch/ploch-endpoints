using Ardalis.Result;

namespace Ploch.Common.WebApi.Endpoints;

/// <summary>
///     Represents a handler for processing a specific type of endpoint operation. The interface is generic over both
///     the request type and the response type, allowing for flexible use across different endpoint implementations.
/// </summary>
/// <typeparam name="TRequest">The type of the request object to be handled by the endpoint.</typeparam>
/// <typeparam name="TResponse">The type of the response object returned by the endpoint.</typeparam>
public interface IEndpointHandler<in TRequest, TResponse>
{
    /// <summary>
    ///     Handles the processing of a request asynchronously.
    /// </summary>
    /// <param name="request">
    ///     The request object containing the necessary data for the operation.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to observe while waiting for the task to complete. The token may signal that the operation should be
    ///     canceled.
    /// </param>
    /// <return>
    ///     A task representing the asynchronous operation, containing the result of the operation as a
    ///     <see cref="Result{TResponse}" />.
    /// </return>
    public Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
