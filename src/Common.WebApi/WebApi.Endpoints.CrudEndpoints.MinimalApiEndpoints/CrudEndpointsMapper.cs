using System.Net;
using Ploch.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Ploch.Common.WebApi.Endpoints.Models;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.MinimalApiEndpoints;
public static class CrudEndpointsMapper
{
    public static IEndpointRouteBuilder MapCrudEndpoints<TEntity, TId, TDto>(this IEndpointRouteBuilder endpoints,
                                                                             string? basePath = null,
                                                                             Action<RouteGroupBuilder>? additionalMappings = null)
        where TEntity : class, IHasId<TId>
    {
        basePath ??= GetBasePath<TEntity>();
        var routeGroupBuilder = endpoints.MapGroup(basePath).WithTags(typeof(TEntity).Name);
        routeGroupBuilder.MapGetById<TEntity, TId, TDto>();
        routeGroupBuilder.MapGetList<TEntity, TId, TDto>();
        routeGroupBuilder.MapCreate<TEntity, TId, TDto>();
        routeGroupBuilder.MapUpdate<TEntity, TId, TDto>();
        routeGroupBuilder.MapDelete<TEntity, TId>();

        additionalMappings?.Invoke(routeGroupBuilder);

        return endpoints;
    }

    public static RouteGroupBuilder MapGetById<TEntity, TId, TDto, TRequest, TResponse>(this RouteGroupBuilder groupBuilder)
        where TEntity : class, IHasId<TId> where TRequest : IdRequest<TId>, new() where TResponse : DataTransferObjectResponse<TDto>
    {
        groupBuilder.MapGet("/{id}",
                            GetByIdEndpoints.GetById<TEntity, TId, TDto, TRequest, TResponse>)
                    .Produces<TResponse>()
                    .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
                    .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
                    .WithDescription($"Gets {typeof(TEntity).Name} by id")
                    .WithName(GetFormattedName<TEntity>("GetById"));

        return groupBuilder;
    }

    public static RouteGroupBuilder MapGetById<TEntity, TId, TDto>(this RouteGroupBuilder groupBuilder)
        where TEntity : class, IHasId<TId>
    {
        groupBuilder.MapGet("/{id}",
                            GetByIdEndpoints.GetById<TEntity, TId, TDto>)
                    .Produces<DataTransferObjectResponse<TDto>>()
                    .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
                    .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
                    .WithDescription($"Gets {typeof(TEntity).Name} by id")
                    .WithName(GetFormattedName<TEntity>("GetById"));

        return groupBuilder;
    }

    public static RouteGroupBuilder MapGetList<TEntity, TId, TDto, TRequest, TResponse>(this RouteGroupBuilder groupBuilder, string methodName = "GetList")
        where TEntity : class, IHasId<TId> where TRequest : PaginatedRequest where TResponse : PaginatedResponse<TDto>
    {
        groupBuilder.MapGet($"/{methodName}",
                            GetListEndpoints.GetList<TEntity, TId, TDto, TRequest, TResponse>)
                    .Produces<TResponse>()
                    .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
                    .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
                    .WithDescription($"Gets a list of {typeof(TEntity).Name}")
                    .WithName(GetFormattedName<TEntity>(methodName));

        return groupBuilder;
    }

    public static RouteGroupBuilder MapGetList<TEntity, TId, TDto>(this RouteGroupBuilder groupBuilder)
        where TEntity : class, IHasId<TId>
    {
        groupBuilder.MapGet("/", GetListEndpoints.GetList<TEntity, TId, TDto>)
                    .Produces<PaginatedResponse<TDto>>()
                    .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
                    .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
                    .WithDescription($"Gets a list of {typeof(TEntity).Name}")
                    .WithName(GetFormattedName<TEntity>("GetList"));

        return groupBuilder;
    }

    public static RouteGroupBuilder MapCreate<TEntity, TId, TDto, TRequest, TResponse>(this RouteGroupBuilder groupBuilder)
        where TEntity : class, IHasId<TId> where TRequest : DataTransferObjectRequest<TDto> where TResponse : DataTransferObjectResponse<TDto>
    {
        groupBuilder.MapPost($"{GetBasePath<TEntity>()}",
                             CreateEndpoints.Create<TEntity, TId, TDto, TRequest, TResponse>)
                    .Produces<TResponse>((int)HttpStatusCode.Created)
                    .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
                    .WithDescription($"Adds {typeof(TEntity).Name} to data store")
                    .WithName(GetFormattedName<TEntity>("Create"));

        return groupBuilder;
    }

    public static RouteGroupBuilder MapCreate<TEntity, TId, TDto>(this RouteGroupBuilder groupBuilder)
        where TEntity : class, IHasId<TId>
    {
        groupBuilder.MapPost(string.Empty,
                             CreateEndpoints.Create<TEntity, TId, TDto>)
                    .Produces((int)HttpStatusCode.NoContent)
                    .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
                    .WithDescription($"Adds {typeof(TEntity).Name} to data store")
                    .WithName(GetFormattedName<TEntity>("Create"));

        return groupBuilder;
    }

    public static RouteGroupBuilder MapUpdate<TEntity, TId, TDto, TRequest, TResponse>(this RouteGroupBuilder groupBuilder)
        where TEntity : class, IHasId<TId> where TRequest : DataTransferObjectRequest<TDto> where TResponse : EmptyResponse
    {
        groupBuilder.MapPut($"{GetBasePath<TEntity>()}", UpdateEndpoints.Update<TEntity, TId, TDto, TRequest, TResponse>)
                    .Produces<TResponse>()
                    .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
                    .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
                    .WithDescription($"Updates {typeof(TEntity).Name} in data store")
                    .WithName(GetFormattedName<TEntity>("Update"));

        return groupBuilder;
    }

    public static RouteGroupBuilder MapUpdate<TEntity, TId, TDto>(this RouteGroupBuilder groupBuilder)
        where TEntity : class, IHasId<TId>
    {
        groupBuilder.MapPut("", UpdateEndpoints.Update<TEntity, TId, TDto>)
                    .Produces((int)HttpStatusCode.NoContent)
                    .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
                    .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
                    .WithDescription($"Updates {typeof(TEntity).Name} in data store")
                    .WithName(GetFormattedName<TEntity>("Update"));

        return groupBuilder;
    }

    public static RouteGroupBuilder MapDelete<TEntity, TId, TRequest, TResponse>(this RouteGroupBuilder groupBuilder)
        where TEntity : class, IHasId<TId> where TRequest : IdRequest<TId> where TResponse : EmptyResponse
    {
        groupBuilder.MapDelete($"{GetBasePath<TEntity>()}", DeleteEndpoints.Delete<TEntity, TId, TRequest, TResponse>)
                    .Produces<TResponse>()
                    .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
                    .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
                    .WithDescription($"Deletes {typeof(TEntity).Name} in data store")
                    .WithName(GetFormattedName<TEntity>("Delete"));

        return groupBuilder;
    }

    public static RouteGroupBuilder MapDelete<TEntity, TId>(this RouteGroupBuilder groupBuilder)
        where TEntity : class, IHasId<TId>
    {
        groupBuilder.MapDelete("/{id:int}", DeleteEndpoints.Delete<TEntity, TId>)
                    .Produces((int)HttpStatusCode.NoContent)
                    .Produces<ProblemDetails>((int)HttpStatusCode.NotFound)
                    .Produces<ProblemDetails>((int)HttpStatusCode.BadRequest)
                    .WithDescription($"Deletes {typeof(TEntity).Name} in data store")
                    .WithName(GetFormattedName<TEntity>("Delete"));

        return groupBuilder;
    }

    private static string GetBasePath<TEntity>() => $"/{typeof(TEntity).Name.ToLowerInvariant()}s";
    private static string GetFormattedName<TEntity>(string methodName) => $"{typeof(TEntity).Name}_{methodName}";
}
