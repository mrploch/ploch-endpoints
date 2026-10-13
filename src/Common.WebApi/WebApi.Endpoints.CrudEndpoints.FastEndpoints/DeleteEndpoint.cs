using System.Net;
using FastEndpoints;
using Ploch.Data.Model;
using FastEndpoints.Swagger;
using Ploch.Common.Ardalis.Result;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Delete;
using EmptyResponse = Ploch.Common.WebApi.Endpoints.Models.EmptyResponse;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.FastEndpoints;
public class DeleteEndpoint<TEntity, TId>(IDeleteEndpointHandler<TEntity, TId> endpointHandler) : Endpoint<IdRequest<TId>, EmptyResponse>
    where TEntity : class, IHasId<TId>
{
    public override void Configure()
    {
        Delete($"/api/{typeof(TEntity).Name.ToLowerInvariant()}/{{Id}}");
        Description(builder => builder.Produces((int)HttpStatusCode.NoContent).WithGroupName(typeof(TEntity).Name).AutoTagOverride(typeof(TEntity).Name));
    }

    public override async Task HandleAsync(IdRequest<TId> req, CancellationToken ct)
    {
        var result = await endpointHandler.HandleAsync(req, ct);

        await SendAsync(result.Value, result.Status.ToHttpStatusCode());
    }
}
