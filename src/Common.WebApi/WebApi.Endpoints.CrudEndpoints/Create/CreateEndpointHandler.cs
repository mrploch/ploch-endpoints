using AutoMapper;
using Ardalis.Result;
using Ploch.Data.Model;
using Ploch.Data.GenericRepository;
using Ploch.Common.WebApi.Endpoints.Models;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.Create;
public abstract class CreateEndpointHandler<TEntity, TId, TDto, TRequest, TResponse>(IUnitOfWork unitOfWork, IMapper mapper)
    : ICreateEndpointHandler<TEntity, TId, TDto, TRequest, TResponse>
    where TEntity : class, IHasId<TId>
    where TRequest : DataTransferObjectRequest<TDto>
    where TResponse : DataTransferObjectResponse<TDto>
{
    public async Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<TEntity>(request.Data);

        var repository = unitOfWork.Repository<TEntity, TId>();

        var added = await AddAsync(repository, request, entity, cancellationToken);

        var addedDto = mapper.Map<TDto>(added);

        return Result.Created(CreateResponse(addedDto));
    }

    protected abstract TResponse CreateResponse(TDto addedDto);

    protected virtual Task<TEntity> AddAsync(IReadWriteRepositoryAsync<TEntity, TId> repository, TRequest reqest, TEntity entity, CancellationToken cancellationToken) =>
        repository.AddAsync(entity, cancellationToken);
}

public class CreateEndpointHandler<TEntity, TId, TDto>(IUnitOfWork unitOfWork, IMapper mapper)
    : CreateEndpointHandler<TEntity, TId, TDto, DataTransferObjectRequest<TDto>, DataTransferObjectResponse<TDto>>(unitOfWork, mapper),
      ICreateEndpointHandler<TEntity, TId, TDto>
    where TEntity : class, IHasId<TId>
{
    protected override DataTransferObjectResponse<TDto> CreateResponse(TDto addedDto) => new(addedDto);
}
