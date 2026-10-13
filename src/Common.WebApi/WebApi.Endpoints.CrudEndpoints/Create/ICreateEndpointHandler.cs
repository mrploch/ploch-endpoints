using Ploch.Data.Model;
using Ploch.Common.WebApi.Endpoints.Models;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.Create;
public interface ICreateEndpointHandler<TEntity, TId, TDto, TRequest, TResponse> : IEndpointHandler<TRequest, TResponse>
    where TEntity : class, IHasId<TId>
    where TRequest : DataTransferObjectRequest<TDto>
    where TResponse : DataTransferObjectResponse<TDto>
{ }

public interface ICreateEndpointHandler<TEntity, TId, TDto> : ICreateEndpointHandler<TEntity, TId, TDto, DataTransferObjectRequest<TDto>,
    DataTransferObjectResponse<TDto>>
    where TEntity : class, IHasId<TId>
{ }
