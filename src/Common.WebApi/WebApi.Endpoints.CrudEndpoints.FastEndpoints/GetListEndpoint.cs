using System.Net;
using FastEndpoints;
using Ploch.Data.Model;
using FastEndpoints.Swagger;
using Ploch.Common.Ardalis.Result;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetAll;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.FastEndpoints;
public class GetListEndpoint<TEntity, TId, TDto>(IGetListEndpointHandler<TEntity, TId, TDto> endpointHandler) : Endpoint<PaginatedRequest, PaginatedResponse<TDto>>
    where TEntity : class, IHasId<TId>
{
    public override void Configure()
    {
        Get($"/api/{typeof(TEntity).Name.ToLowerInvariant()}s");
        Description(builder => builder.Produces<TDto>().Produces((int)HttpStatusCode.NotFound).AutoTagOverride(typeof(TEntity).Name).WithGroupName(typeof(TEntity).Name));
    }

    public override async Task HandleAsync(PaginatedRequest req, CancellationToken ct)
    {
        var result = await endpointHandler.HandleAsync(req, ct);

        await SendAsync(result.Value, result.Status.ToHttpStatusCode());
    }
}
