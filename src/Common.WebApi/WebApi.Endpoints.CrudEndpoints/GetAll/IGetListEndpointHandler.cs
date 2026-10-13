using Ploch.Data.Model;
using Ploch.Common.WebApi.Endpoints.Models;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetAll;
public interface IGetListEndpointHandler<TEntity, TId, TDto, TPaginatedRequest, TPaginatedResponse> : IEndpointHandler<TPaginatedRequest, TPaginatedResponse>
    where TEntity : class, IHasId<TId> where TPaginatedRequest : PaginatedRequest where TPaginatedResponse : PaginatedResponse<TDto>
{ }

public interface IGetListEndpointHandler<TEntity, TId, TDto> : IGetListEndpointHandler<TEntity, TId, TDto, PaginatedRequest, PaginatedResponse<TDto>>
    where TEntity : class, IHasId<TId>
{ }
