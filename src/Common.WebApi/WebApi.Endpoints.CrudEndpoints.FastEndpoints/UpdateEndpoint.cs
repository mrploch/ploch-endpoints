using System.Net;
using FastEndpoints;
using Ploch.Data.Model;
using FastEndpoints.Swagger;
using Ploch.Common.Ardalis.Result;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Update;
using EmptyResponse = Ploch.Common.WebApi.Endpoints.Models.EmptyResponse;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.FastEndpoints;
public class UpdateEndpoint<TEntity, TId, TDto>(IUpdateEndpointHandler<TEntity, TId, TDto> endpointHandler) : Endpoint<DataTransferObjectRequest<TDto>, EmptyResponse>
    where TEntity : class, IHasId<TId>
{
    public override void Configure()
    {
        Post($"/api/{typeof(TEntity).Name.ToLowerInvariant()}");
        Description(builder => builder.Produces((int)HttpStatusCode.NoContent)
                                      .Produces((int)HttpStatusCode.NotFound)
                                      .AutoTagOverride(typeof(TEntity).Name)
                                      .WithGroupName(typeof(TEntity).Name));
    }

    public override async Task HandleAsync(DataTransferObjectRequest<TDto> req, CancellationToken ct)
    {
        var result = await endpointHandler.HandleAsync(req, ct);

        await SendAsync(result.Value, result.Status.ToHttpStatusCode(), ct);
    }
}
