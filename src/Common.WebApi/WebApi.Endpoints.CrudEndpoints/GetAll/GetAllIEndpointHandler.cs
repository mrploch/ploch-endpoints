using AutoMapper;
using Ardalis.Result;
using Ploch.Data.Model;
using System.Linq.Expressions;
using Ploch.Data.GenericRepository;
using Ploch.Common.WebApi.Endpoints.Models;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetAll;
public class GetListEndpointHandler<TEntity, TId, TDto>(IReadRepositoryAsync<TEntity, TId> repository, IMapper mapper, EntityQueryOperations<TEntity> operations)
    : GetListEndpointHandler<TEntity, TId, TDto, PaginatedRequest>(repository, mapper, operations), IGetListEndpointHandler<TEntity, TId, TDto>
    where TEntity : class, IHasId<TId>
{ }

public class GetListEndpointHandler<TEntity, TId, TDto, TPaginatedRequest>(
    IReadRepositoryAsync<TEntity, TId> repository,
    IMapper mapper,
    EntityQueryOperations<TEntity> operations)
    : GetListEndpointHandler<TEntity, TId, TDto, TPaginatedRequest, PaginatedResponse<TDto>>(repository, mapper, operations)
    where TEntity : class, IHasId<TId> where TPaginatedRequest : PaginatedRequest
{
    protected override Result<PaginatedResponse<TDto>> CreateResponse(IEnumerable<TDto> dtos, int? count, bool moreItems, TPaginatedRequest request) =>
        new(new PaginatedResponse<TDto>(dtos, request.PageNumber, request.PageSize, count, moreItems));
}

public abstract class GetListEndpointHandler<TEntity, TId, TDto, TPaginatedRequest, TPaginatedResponse>(
    IReadRepositoryAsync<TEntity, TId> repository,
    IMapper mapper,
    EntityQueryOperations<TEntity> operations)
    : IGetListEndpointHandler<TEntity, TId, TDto, TPaginatedRequest, TPaginatedResponse>
    where TEntity : class, IHasId<TId> where TPaginatedRequest : PaginatedRequest where TPaginatedResponse : PaginatedResponse<TDto>
{
    public virtual async Task<Result<TPaginatedResponse>> HandleAsync(TPaginatedRequest request, CancellationToken cancellationToken)
    {
        var queryFilter = GetQueryFilter(request);

        var count = await GetCountAsync(queryFilter, cancellationToken);

        if (request.PageNumber * request.PageNumber > count)
        {
            return Result.NotFound("Total items are less than provided page.");
        }

        var entities = await GetEntitiesAsync(request, queryFilter, cancellationToken);

        var dtos = mapper.Map<IEnumerable<TDto>>(entities);
        var moreItems = request.PageNumber * request.PageSize < count;

        return CreateResponse(dtos, count, moreItems, request);
    }

    protected virtual Expression<Func<TEntity, bool>>? GetQueryFilter(TPaginatedRequest request) => null;

    protected abstract Result<TPaginatedResponse> CreateResponse(IEnumerable<TDto> dtos, int? count, bool moreItems, TPaginatedRequest request);

    protected virtual async Task<IEnumerable<TEntity>> GetEntitiesAsync(TPaginatedRequest request,
                                                                        Expression<Func<TEntity, bool>>? queryFilter,
                                                                        CancellationToken cancellationToken) =>
        await repository.GetPageAsync(request.PageNumber,
                                      request.PageSize,
                                      onDbSet: operations.GetListOperation,
                                      query: queryFilter,
                                      cancellationToken: cancellationToken);

    protected virtual async Task<int?> GetCountAsync(Expression<Func<TEntity, bool>>? queryFilter, CancellationToken cancellationToken) =>
        queryFilter == null ? await repository.CountAsync(cancellationToken: cancellationToken) : await repository.CountAsync(queryFilter, cancellationToken);
}
