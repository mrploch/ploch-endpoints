using Ploch.Data.Model;
using Ploch.Common.WebApi.Endpoints.Models;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.Update;
public interface IUpdateEndpointHandler<TEntity, TId, TDto, TUpdateRequest, TUpdateResponse> : IEndpointHandler<TUpdateRequest, TUpdateResponse>
    where TEntity : class, IHasId<TId>
    where TUpdateRequest : DataTransferObjectRequest<TDto>
    where TUpdateResponse : EmptyResponse
{ }

public interface IUpdateEndpointHandler<TEntity, TId, TDto> : IUpdateEndpointHandler<TEntity, TId, TDto, DataTransferObjectRequest<TDto>, EmptyResponse>
    where TEntity : class, IHasId<TId>
{ }
