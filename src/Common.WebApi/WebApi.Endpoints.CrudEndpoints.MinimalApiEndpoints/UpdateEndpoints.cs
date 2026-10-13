using Ploch.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Ardalis.Result.AspNetCore;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Update;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.MinimalApiEndpoints;
public static class UpdateEndpoints
{
    public static async Task<IResult> Update<TEntity, TId, TDto, TRequest, TResponse>(TRequest request,
                                                                                      IUpdateEndpointHandler<TEntity, TId, TDto, TRequest, TResponse> handler,
                                                                                      CancellationToken cancellationToken)
        where TEntity : class, IHasId<TId> where TRequest : DataTransferObjectRequest<TDto> where TResponse : EmptyResponse =>
        (await handler.HandleAsync(request, cancellationToken)).ToMinimalApiResult();

    public static async Task<IResult> Update<TEntity, TId, TDto>([FromBody] DataTransferObjectRequest<TDto> request,
                                                                 IUpdateEndpointHandler<TEntity, TId, TDto> handler,
                                                                 CancellationToken cancellationToken)
        where TEntity : class, IHasId<TId> =>
        (await handler.HandleAsync(request, cancellationToken)).ToMinimalApiResult();
}
