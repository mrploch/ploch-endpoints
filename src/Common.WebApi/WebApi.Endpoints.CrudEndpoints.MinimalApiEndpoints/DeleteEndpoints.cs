using Ploch.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Ardalis.Result.AspNetCore;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Delete;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.MinimalApiEndpoints;
public static class DeleteEndpoints
{
    public static async Task<IResult> Delete<TEntity, TId, TRequest, TResponse>(TRequest request,
                                                                                IDeleteEndpointHandler<TEntity, TId, TRequest, TResponse> handler,
                                                                                CancellationToken cancellationToken)
        where TEntity : class, IHasId<TId>
        where TRequest : IdRequest<TId>
        where TResponse : EmptyResponse =>
        (await handler.HandleAsync(request, cancellationToken)).ToMinimalApiResult();

    public static async Task<IResult> Delete<TEntity, TId>([FromRoute] TId id,
                                                           IDeleteEndpointHandler<TEntity, TId> handler,
                                                           CancellationToken cancellationToken)
        where TEntity : class, IHasId<TId> =>
        (await handler.HandleAsync(new IdRequest<TId> { Id = id }, cancellationToken)).ToMinimalApiResult();
}
