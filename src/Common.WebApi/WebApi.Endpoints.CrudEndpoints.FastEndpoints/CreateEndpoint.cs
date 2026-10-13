using System.Net;
using FastEndpoints;
using Ploch.Data.Model;
using FastEndpoints.Swagger;
using Ploch.Common.Ardalis.Result;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Create;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.FastEndpoints;
public class CreateEndpoint<TEntity, TId, TDto>(ICreateEndpointHandler<TEntity, TId, TDto> endpointHandler)
    : Endpoint<DataTransferObjectRequest<TDto>, DataTransferObjectResponse<TDto>>
    where TEntity : class, IHasId<TId>
{
    public override void Configure()
    {
        Put($"/api/{typeof(TEntity).Name.ToLowerInvariant()}");
        Description(builder =>
                    {
                        builder.ClearDefaultProduces(200)
                               .Produces<DataTransferObjectResponse<TDto>>((int)HttpStatusCode.Created)
                               .AutoTagOverride(typeof(TEntity).Name)
                               .WithGroupName(typeof(TEntity).Name)
                               .WithSummary($"Creates a new {typeof(TEntity).Name}");
                    });
        Summary(summary => summary.Summary = $"Creates a new {typeof(TEntity).Name}");
    }

    public override async Task HandleAsync(DataTransferObjectRequest<TDto> req, CancellationToken ct)
    {
        var result = await endpointHandler.HandleAsync(req, ct);

        await SendAsync(result.Value, result.Status.ToHttpStatusCode());
    }
}
