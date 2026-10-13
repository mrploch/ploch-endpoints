using Ploch.Data.Model;
using Ploch.Common.WebApi.Endpoints.Models;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetById;
public interface IGetByIdEndpointHandler<TEntity, TId, TDto, TGetByIdRequest, TGetByIdResponse> : IEndpointHandler<TGetByIdRequest, TGetByIdResponse>
    where TEntity : class, IHasId<TId> where TGetByIdRequest : IdRequest<TId> where TGetByIdResponse : DataTransferObjectResponse<TDto>
{ }

public interface IGetByIdEndpointHandler<TEntity, TId, TDto, TGetByIdRequest> : IGetByIdEndpointHandler<TEntity, TId, TDto, TGetByIdRequest,
    DataTransferObjectResponse<TDto>>
    where TEntity : class, IHasId<TId> where TGetByIdRequest : IdRequest<TId>
{ }

public interface IGetByIdEndpointHandler<TEntity, TId, TDto> : IGetByIdEndpointHandler<TEntity, TId, TDto, IdRequest<TId>>
    where TEntity : class, IHasId<TId>
{ }
