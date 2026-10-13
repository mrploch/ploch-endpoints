using System.Net;
using FastEndpoints;
using Ploch.Data.Model;
using FastEndpoints.Swagger;
using Ploch.Common.Ardalis.Result;
using Ploch.Common.WebApi.Endpoints.Models;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetById;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.FastEndpoints;
public class GetByIdEndpoint<TEntity, TId, TDto>(IGetByIdEndpointHandler<TEntity, TId, TDto> endpointHandler) : Endpoint<IdRequest<TId>, DataTransferObjectResponse<TDto>>
    where TEntity : class, IHasId<TId>
{
    public override void Configure()
    {
        Get($"/api/{typeof(TEntity).Name.ToLowerInvariant()}/{{Id}}");
        Description(builder => builder.Produces<TDto>()
                                      .Produces((int)HttpStatusCode.NoContent)
                                      .AutoTagOverride(typeof(TEntity).Name)
                                      .WithGroupName(typeof(TEntity).Name));
    }

    public override async Task HandleAsync(IdRequest<TId> req, CancellationToken ct)
    {
        var result = await endpointHandler.HandleAsync(req, ct);

        await SendAsync(result.Value, result.Status.ToHttpStatusCode());
    }
}
