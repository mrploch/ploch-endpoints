using Ardalis.Result;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Data.GenericRepository;
using Ploch.Data.Model;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetById;

/// <summary>
///     Base handler that returns a single entity by its identifier, mapped to a data transfer object.
/// </summary>
/// <remarks>
///     Retrieved entities are cached for five minutes, keyed by entity type and identifier.
/// </remarks>
/// <typeparam name="TEntity">The entity type being retrieved.</typeparam>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TDto">The data transfer object the entity is mapped to.</typeparam>
/// <typeparam name="TGetByIdRequest">The request type carrying the identifier.</typeparam>
/// <typeparam name="TGetByIdResponse">The response type wrapping the mapped data transfer object.</typeparam>
/// <param name="repository">The repository the entity is read from.</param>
/// <param name="mapper">The mapper used to project the entity onto <typeparamref name="TDto" />.</param>
/// <param name="operations">Query operations applied when reading the entity, such as includes and projections.</param>
/// <param name="cache">The cache consulted before, and populated after, a repository read.</param>
public abstract class GetByIdEndpointHandler<TEntity, TId, TDto, TGetByIdRequest, TGetByIdResponse>(
    IReadRepositoryAsync<TEntity, TId> repository,
    IMapper mapper,
    EntityQueryOperations<TEntity> operations,
    IMemoryCache cache)
    : IGetByIdEndpointHandler<TEntity, TId, TDto, TGetByIdRequest, TGetByIdResponse>
    where TEntity : class, IHasId<TId> where TGetByIdRequest : IdRequest<TId> where TGetByIdResponse : DataTransferObjectResponse<TDto>
{
    /// <summary>
    ///     Retrieves the requested entity and returns it as a mapped response.
    /// </summary>
    /// <param name="request">The request carrying the identifier to look up.</param>
    /// <param name="cancellationToken">A token used to cancel the read.</param>
    /// <returns>
    ///     A successful result carrying the mapped response, or <c>NotFound</c> when no entity has that identifier.
    /// </returns>
    public virtual async Task<Result<TGetByIdResponse>> HandleAsync(TGetByIdRequest request, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue($"{typeof(TEntity).Name}_{request.Id}", out TEntity? cachedEntity) && cachedEntity is not null)
        {
            var fromCachedDto = mapper.Map<TDto>(cachedEntity);

            return CreateResponse(cachedEntity, fromCachedDto, request);
        }

        var entity = await GetEntityAsync(request, cancellationToken);

        if (entity is null)
        {
            return Result<TGetByIdResponse>.NotFound($"Item with id {request.Id} was not found.");
        }

        cache.Set($"{typeof(TEntity).Name}_{request.Id}", entity, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

        var dto = mapper.Map<TDto>(entity);

        return CreateResponse(entity, dto, request);
    }

    /// <summary>
    ///     Reads the entity from the repository, applying the configured query operations when present.
    /// </summary>
    /// <param name="request">The request carrying the identifier to look up.</param>
    /// <param name="cancellationToken">A token used to cancel the read.</param>
    /// <returns>The entity, or <see langword="null" /> when no entity has that identifier.</returns>
    protected virtual Task<TEntity?> GetEntityAsync(TGetByIdRequest request, CancellationToken cancellationToken) =>
        operations.GetByIdOperation != null
            ? repository.GetByIdAsync(request.Id, operations.GetByIdOperation, cancellationToken)
            : repository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);

    /// <summary>
    ///     Builds the response returned to the caller from the retrieved entity and its mapped data transfer object.
    /// </summary>
    /// <param name="entity">The entity that was retrieved.</param>
    /// <param name="dto">The data transfer object <paramref name="entity" /> was mapped to.</param>
    /// <param name="request">The originating request.</param>
    /// <returns>The response to return to the caller.</returns>
    protected abstract TGetByIdResponse CreateResponse(TEntity entity, TDto dto, TGetByIdRequest request);
}

/// <summary>
///     Get-by-id handler that wraps the mapped data transfer object in a
///     <see cref="DataTransferObjectResponse{TDto}" />, so a response type does not have to be declared.
/// </summary>
/// <typeparam name="TEntity">The entity type being retrieved.</typeparam>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TDto">The data transfer object the entity is mapped to.</typeparam>
/// <typeparam name="TGetByIdRequest">The request type carrying the identifier.</typeparam>
/// <param name="repository">The repository the entity is read from.</param>
/// <param name="mapper">The mapper used to project the entity onto <typeparamref name="TDto" />.</param>
/// <param name="operations">Query operations applied when reading the entity.</param>
/// <param name="cache">The cache consulted before, and populated after, a repository read.</param>
public class GetByIdEndpointHandler<TEntity, TId, TDto, TGetByIdRequest>(
    IReadRepositoryAsync<TEntity, TId> repository,
    IMapper mapper,
    EntityQueryOperations<TEntity> operations,
    IMemoryCache cache)
    : GetByIdEndpointHandler<TEntity, TId, TDto, TGetByIdRequest, DataTransferObjectResponse<TDto>>(repository, mapper, operations, cache),
      IGetByIdEndpointHandler<TEntity, TId, TDto, TGetByIdRequest>
    where TEntity : class, IHasId<TId> where TGetByIdRequest : IdRequest<TId>
{
    /// <inheritdoc />
    protected override DataTransferObjectResponse<TDto> CreateResponse(TEntity entity, TDto dto, TGetByIdRequest request) => new(dto);
}

/// <summary>
///     Get-by-id handler for the common case, taking an <see cref="IdRequest{TId}" /> and returning a
///     <see cref="DataTransferObjectResponse{TDto}" />.
/// </summary>
/// <typeparam name="TEntity">The entity type being retrieved.</typeparam>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TDto">The data transfer object the entity is mapped to.</typeparam>
/// <param name="repository">The repository the entity is read from.</param>
/// <param name="mapper">The mapper used to project the entity onto <typeparamref name="TDto" />.</param>
/// <param name="operations">Query operations applied when reading the entity.</param>
/// <param name="cache">The cache consulted before, and populated after, a repository read.</param>
public class GetByIdEndpointHandler<TEntity, TId, TDto>(
    IReadRepositoryAsync<TEntity, TId> repository,
    IMapper mapper,
    EntityQueryOperations<TEntity> operations,
    IMemoryCache cache)
    : GetByIdEndpointHandler<TEntity, TId, TDto, IdRequest<TId>>(repository, mapper, operations, cache),
      IGetByIdEndpointHandler<TEntity, TId, TDto>
    where TEntity : class, IHasId<TId>
{ }
