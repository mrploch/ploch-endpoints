using Ploch.Data.Model;
using Ploch.Common.WebApi.Endpoints.Models;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.Delete;
public interface IDeleteEndpointHandler<TEntity, TId, TDeleteRequest, TDeleteResponse> : IEndpointHandler<TDeleteRequest, TDeleteResponse>
    where TEntity : class, IHasId<TId>
    where TDeleteRequest : IdRequest<TId>
    where TDeleteResponse : EmptyResponse
{ }

public interface IDeleteEndpointHandler<TEntity, TId> : IDeleteEndpointHandler<TEntity, TId, IdRequest<TId>, EmptyResponse>
    where TEntity : class, IHasId<TId>
{ }
