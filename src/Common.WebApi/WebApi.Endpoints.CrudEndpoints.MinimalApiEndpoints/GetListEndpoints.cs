using Ploch.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Ardalis.Result.AspNetCore;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetAll;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.MinimalApiEndpoints;
public static class GetListEndpoints
{
    public static async Task<IResult> GetList<TEntity, TId, TDto, TPaginatedRequest, TPaginatedResponse>([AsParameters] TPaginatedRequest request,
                                                                                                         IGetListEndpointHandler<TEntity, TId, TDto, TPaginatedRequest,
                                                                                                             TPaginatedResponse> handler,
                                                                                                         CancellationToken cancellationToken)
        where TEntity : class, IHasId<TId>
        where TPaginatedRequest : PaginatedRequest
        where TPaginatedResponse : PaginatedResponse<TDto> =>
        (await handler.HandleAsync(request, cancellationToken)).ToMinimalApiResult();

    public static async Task<IResult> GetList<TEntity, TId, TDto>([FromQuery] int pageNumber,
                                                                  [FromQuery] int pageSize,
                                                                  [FromServices] IGetListEndpointHandler<TEntity, TId, TDto> handler,
                                                                  CancellationToken cancellationToken)
        where TEntity : class, IHasId<TId>
    {
        var result = await handler.HandleAsync(new PaginatedRequest { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
        var minimalApiResult = result.ToMinimalApiResult();

        return minimalApiResult;
    }
}
