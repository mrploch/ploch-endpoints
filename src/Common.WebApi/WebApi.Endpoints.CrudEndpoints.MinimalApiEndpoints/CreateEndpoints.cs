using Ploch.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Ardalis.Result.AspNetCore;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Create;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.MinimalApiEndpoints;
public static class CreateEndpoints
{
    public static async Task<IResult> Create<TEntity, TId, TDto, TRequest, TResponse>(TRequest request,
                                                                                      ICreateEndpointHandler<TEntity, TId, TDto, TRequest, TResponse> handler,
                                                                                      CancellationToken cancellationToken)
        where TEntity : class, IHasId<TId>
        where TRequest : DataTransferObjectRequest<TDto>
        where TResponse : DataTransferObjectResponse<TDto> =>
        (await handler.HandleAsync(request, cancellationToken)).ToMinimalApiResult();

    public static async Task<IResult> Create<TEntity, TId, TDto>([FromBody] DataTransferObjectRequest<TDto> request,
                                                                 ICreateEndpointHandler<TEntity, TId, TDto> handler,
                                                                 CancellationToken cancellationToken)
        where TEntity : class, IHasId<TId> =>
        (await handler.HandleAsync(request, cancellationToken)).ToMinimalApiResult();
}
