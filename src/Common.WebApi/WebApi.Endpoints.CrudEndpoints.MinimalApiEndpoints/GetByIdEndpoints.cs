using Ploch.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Ardalis.Result.AspNetCore;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetById;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.MinimalApiEndpoints;
public static class GetByIdEndpoints
{
    public static async Task<IResult> GetById<TEntity, TId, TDto, TRequest, TResponse>([FromRoute] TId id,
                                                                                       IGetByIdEndpointHandler<TEntity, TId, TDto, TRequest, TResponse> handler,
                                                                                       CancellationToken cancellationToken)
        where TEntity : class, IHasId<TId> where TRequest : IdRequest<TId>, new() where TResponse : DataTransferObjectResponse<TDto> =>
        (await handler.HandleAsync(new TRequest { Id = id }, cancellationToken)).ToMinimalApiResult();

    public static async Task<IResult> GetById<TEntity, TId, TDto>([FromRoute] TId id,
                                                                  [FromServices] IGetByIdEndpointHandler<TEntity, TId, TDto> handler,
                                                                  CancellationToken cancellationToken)
        where TEntity : class, IHasId<TId> =>
        (await handler.HandleAsync(new IdRequest<TId> { Id = id }, cancellationToken)).ToMinimalApiResult();
}
