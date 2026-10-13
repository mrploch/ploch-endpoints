using AutoMapper;
using Ardalis.Result;
using Ploch.Data.Model;
using Microsoft.Extensions.Logging;
using Ploch.Data.GenericRepository;
using Ploch.Common.WebApi.Endpoints.Models;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.Update;
public class UpdateEndpointHandler<TEntity, TId, TDto, TUpdateRequest, TUpdateResponse>(
    ILogger<UpdateEndpointHandler<TEntity, TId, TDto, TUpdateRequest, TUpdateResponse>> logger,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IUpdateEndpointHandler<TEntity, TId, TDto, TUpdateRequest, TUpdateResponse>
    where TEntity : class, IHasId<TId> where TUpdateRequest : DataTransferObjectRequest<TDto> where TUpdateResponse : EmptyResponse, new()
{
    public virtual async Task<Result<TUpdateResponse>> HandleAsync(TUpdateRequest request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<TEntity>(request.Data);

        try
        {
            var repository = unitOfWork.Repository<TEntity, TId>();
            await repository.UpdateAsync(entity, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (EntityNotFoundException ex)
        {
            return HandleNotFoundResponse(ex, request, entity);
        }

        return CreateResponse();
    }

    protected virtual Result<TUpdateResponse> HandleNotFoundResponse(EntityNotFoundException exception, TUpdateRequest request, TEntity entity)
    {
        logger.LogWarning(exception, "Entity with id {Id} not found", entity.Id);

        return Result<TUpdateResponse>.NotFound($"Entity with id {entity.Id} not found");
    }

    protected virtual Result<TUpdateResponse> CreateResponse() => Result<TUpdateResponse>.NoContent();
}

public class UpdateEndpointHandler<TEntity, TId, TDto>(
    ILogger<UpdateEndpointHandler<TEntity, TId, TDto>> logger,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : UpdateEndpointHandler<TEntity, TId, TDto, DataTransferObjectRequest<TDto>, EmptyResponse>(logger, unitOfWork, mapper), IUpdateEndpointHandler<TEntity, TId, TDto>
    where TEntity : class, IHasId<TId>
{ }
