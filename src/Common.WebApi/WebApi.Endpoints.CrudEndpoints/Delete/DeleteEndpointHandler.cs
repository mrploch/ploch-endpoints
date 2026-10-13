using Ardalis.Result;
using Ploch.Data.Model;
using Microsoft.Extensions.Logging;
using Ploch.Data.GenericRepository;
using Ploch.Common.WebApi.Endpoints.Models;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.Delete;
public class DeleteEndpointHandler<TEntity, TId, TDeleteRequest, TDeleteResponse>(
    IUnitOfWork unitOfWork,
    ILogger<DeleteEndpointHandler<TEntity, TId, TDeleteRequest, TDeleteResponse>> logger)
    : IDeleteEndpointHandler<TEntity, TId, TDeleteRequest, TDeleteResponse>
    where TEntity : class, IHasId<TId> where TDeleteRequest : IdRequest<TId> where TDeleteResponse : EmptyResponse, new()
{
    public async Task<Result<TDeleteResponse>> HandleAsync(TDeleteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var repository = unitOfWork.Repository<TEntity, TId>();
            await repository.DeleteAsync(request.Id, cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (EntityNotFoundException ex)
        {
            return HandleNotFoundResponse(ex, request);
        }

        return CreateSuccessResult();
    }

    protected virtual Result<TDeleteResponse> CreateSuccessResult() => Result.NoContent();

    protected virtual Result<TDeleteResponse> HandleNotFoundResponse(EntityNotFoundException exception, TDeleteRequest request)
    {
        logger.LogWarning(exception, "Entity with id {Id} not found", request.Id);

        return Result<TDeleteResponse>.NotFound();
    }
}

public class DeleteEndpointHandler<TEntity, TId>(IUnitOfWork unitOfWork, ILogger<DeleteEndpointHandler<TEntity, TId>> logger)
    : DeleteEndpointHandler<TEntity, TId, IdRequest<TId>, EmptyResponse>(unitOfWork, logger), IDeleteEndpointHandler<TEntity, TId>
    where TEntity : class, IHasId<TId>
{ }
