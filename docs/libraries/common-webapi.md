# Ploch.Common.WebApi

> A suite of NuGet packages providing generic CRUD endpoint handlers, request/response models, and OpenAPI configuration for ASP.NET Core Web APIs.

## Overview

`Ploch.Common.WebApi` is an umbrella grouping of related libraries that reduce the boilerplate required to build RESTful CRUD APIs in ASP.NET Core. The group is split across several focused packages:

- **`Ploch.Common.WebApi`** — OpenAPI/Swagger configuration helpers and a `QueryStringBinder` utility.
- **`Ploch.Common.WebApi.Endpoints`** — Core `IEndpointHandler<TRequest, TResponse>` abstraction and shared request/response model types (`PaginatedRequest`, `PaginatedResponse<T>`, `IdRequest<TId>`, `DataTransferObjectRequest<T>`, etc.).
- **`Ploch.Common.WebApi.Endpoints.CrudEndpoints`** — Full CRUD handler implementations backed by `Ploch.Data.GenericRepository`: `GetListEndpointHandler`, `GetByIdEndpointHandler`, `CreateEndpointHandler`, `UpdateEndpointHandler`, and `DeleteEndpointHandler`. Includes a fluent `CrudEndpointsBuilder` DI registration API.
- **`Ploch.Common.WebApi.Endpoints.CrudEndpoints.MinimalApiEndpoints`** — Minimal API route-mapping layer on top of the CRUD handlers (`CrudEndpointsMapper`).
- **`Ploch.Common.WebApi.Endpoints.CrudEndpoints.FastEndpoints`** — FastEndpoints-based endpoint classes that delegate to the same CRUD handlers.

The handlers integrate AutoMapper for entity-to-DTO mapping and EF Core generic repositories for data access. `GetByIdEndpointHandler` includes built-in `IMemoryCache` support to avoid repeated database round-trips for frequently accessed entities.

Results are returned as `Ardalis.Result<T>` throughout the handler layer, which maps cleanly to HTTP status codes at the framework integration layer (Minimal API or FastEndpoints).

## Installation

Install the package that matches your endpoint framework:

```shell
# Core abstractions (request/response models, IEndpointHandler)
dotnet add package Ploch.Common.WebApi.Endpoints

# CRUD handlers + DI builder (framework-agnostic)
dotnet add package Ploch.Common.WebApi.Endpoints.CrudEndpoints

# Minimal API route mapping
dotnet add package Ploch.Common.WebApi.Endpoints.CrudEndpoints.MinimalApiEndpoints

# FastEndpoints integration
dotnet add package Ploch.Common.WebApi.Endpoints.CrudEndpoints.FastEndpoints

# OpenAPI helpers and QueryStringBinder
dotnet add package Ploch.Common.WebApi
```

## Key Types

### Core abstractions (`Ploch.Common.WebApi.Endpoints`)

| Type | Kind | Description |
|---|---|---|
| `IEndpointHandler<TRequest, TResponse>` | Interface | Base contract for all endpoint handlers. Returns `Task<Result<TResponse>>`. |
| `PaginatedRequest` | Class | Carries `PageNumber` (default 1) and `PageSize` (default 10). |
| `PaginatedResponse<TDto>` | Class | Wraps an `IEnumerable<TDto>` page with `PageNumber`, `PageSize`, `TotalItems?`, and `MoreItems?`. |
| `IdRequest<TId>` | Class | Simple request carrying a single `Id` property. |
| `DataTransferObjectRequest<TDto>` | Class | Wraps a DTO in a `Data` property for create/update requests. |
| `DataTransferObjectResponse<TDto>` | Class | Wraps a DTO in a `Data` property for create/get-by-id responses. |
| `EmptyResponse` | Class | Placeholder response for operations that return no body (delete, update). |

### CRUD handlers (`Ploch.Common.WebApi.Endpoints.CrudEndpoints`)

| Type | Kind | Description |
|---|---|---|
| `ServiceCollectionRegistration` | Static class | Extension method `AddCrudEndpoints()` — entry point for the fluent DI builder. |
| `CrudEndpointsBuilder` | Class | Fluent builder: `WithAutoMapper<TProfile>()`, `WithDbContext<TDbContext>()`, `WithMemoryCache()`, `MapEndpoints()`. |
| `EntityTypeEndpointMapper` | Class | Produced by `MapEndpoints()`. Call `MapType<TEntity, TId, TDto>()` once per entity. |
| `EntityQueryOperations<TEntity>` | Class | Optional per-entity `Func<IQueryable<T>, IQueryable<T>>` hooks for get-list, get-by-id, and update queries (useful for eager loading). |
| `GetListEndpointHandler<TEntity, TId, TDto>` | Class | Paginated list handler. Implements `IGetListEndpointHandler<TEntity, TId, TDto>`. |
| `GetByIdEndpointHandler<TEntity, TId, TDto>` | Class | Single-entity handler with memory cache. Implements `IGetByIdEndpointHandler<TEntity, TId, TDto>`. |
| `CreateEndpointHandler<TEntity, TId, TDto>` | Class | Create handler. Maps DTO to entity, persists via `IUnitOfWork`. |
| `UpdateEndpointHandler<TEntity, TId, TDto>` | Class | Update handler. Maps DTO, calls `UpdateAsync`, commits. |
| `DeleteEndpointHandler<TEntity, TId>` | Class | Delete handler. Deletes by ID, commits. Returns `NotFound` on missing entity. |

### Minimal API integration (`Ploch.Common.WebApi.Endpoints.CrudEndpoints.MinimalApiEndpoints`)

| Type | Kind | Description |
|---|---|---|
| `CrudEndpointsMapper` | Static class | Extension methods on `IEndpointRouteBuilder`: `MapCrudEndpoints<TEntity, TId, TDto>()`, and individual `MapGetById`, `MapGetList`, `MapCreate`, `MapUpdate`, `MapDelete` overloads. |

### FastEndpoints integration (`Ploch.Common.WebApi.Endpoints.CrudEndpoints.FastEndpoints`)

| Type | Kind | Description |
|---|---|---|
| `CreateEndpoint<TEntity, TId, TDto>` | Class | FastEndpoints `Endpoint` that delegates to `ICreateEndpointHandler`. |
| `GetByIdEndpoint<TEntity, TId, TDto>` | Class | FastEndpoints `Endpoint` that delegates to `IGetByIdEndpointHandler`. |
| `GetListEndpoint<TEntity, TId, TDto>` | Class | FastEndpoints `Endpoint` that delegates to `IGetListEndpointHandler`. |
| `UpdateEndpoint<TEntity, TId, TDto>` | Class | FastEndpoints `Endpoint` that delegates to `IUpdateEndpointHandler`. |
| `DeleteEndpoint<TEntity, TId>` | Class | FastEndpoints `Endpoint` that delegates to `IDeleteEndpointHandler`. |

### OpenAPI utilities (`Ploch.Common.WebApi`)

| Type | Kind | Description |
|---|---|---|
| `OpenApiConfigurator` | Static class | Extension method `ConfigureOpenApiOptions(OpenApiInfo, string)` — configures SwaggerGen with custom operation IDs. |
| `QueryStringBinder` | Static class | `Bind<TQuery>(HttpContext)` and `TryParse<TQuery>(IDictionary<string, StringValues>, out TQuery)` — reflectively maps query-string parameters to a POCO. |

## Usage Examples

### Standard setup with Minimal API

Register the CRUD stack in `Program.cs` and map routes:

```csharp
// Program.cs
builder.Services
    .AddCrudEndpoints()
    .WithAutoMapper<BlogPostProfile>()
    .WithDbContext<BlogDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")))
    .WithMemoryCache()
    .MapEndpoints()
    .MapType<BlogPost, int, BlogPostDto>();

var app = builder.Build();

app.MapCrudEndpoints<BlogPost, int, BlogPostDto>();

app.Run();
```

This registers the five handler services and maps the following routes under `/blogposts`:

| Method | Route | Handler |
|---|---|---|
| `GET` | `/blogposts/{id}` | `GetByIdEndpointHandler` |
| `GET` | `/blogposts/` | `GetListEndpointHandler` |
| `POST` | `/blogposts` | `CreateEndpointHandler` |
| `PUT` | `/blogposts` | `UpdateEndpointHandler` |
| `DELETE` | `/blogposts/{id:int}` | `DeleteEndpointHandler` |

### Eager loading with EntityQueryOperations

Provide an `EntityQueryOperations<TEntity>` instance to configure `Include` calls for list and get-by-id queries:

```csharp
.MapType<BlogPost, int, BlogPostDto>(new EntityQueryOperations<BlogPost>
{
    GetListOperation = q => q.Include(p => p.Tags).Include(p => p.Categories),
    GetByIdOperation = q => q.Include(p => p.Tags).Include(p => p.Author)
})
```

### Custom handler by inheritance

Override `GetQueryFilter` to add server-side filtering to the list handler:

```csharp
public class PublishedBlogPostListHandler(
    IReadRepositoryAsync<BlogPost, int> repository,
    IMapper mapper,
    EntityQueryOperations<BlogPost> operations)
    : GetListEndpointHandler<BlogPost, int, BlogPostDto>(repository, mapper, operations)
{
    protected override Expression<Func<BlogPost, bool>>? GetQueryFilter(PaginatedRequest request)
        => post => post.IsPublished;
}
```

Register the override before calling `MapType`:

```csharp
services.AddScoped<IGetListEndpointHandler<BlogPost, int, BlogPostDto>, PublishedBlogPostListHandler>();
```

### Binding query string parameters

```csharp
app.MapGet("/search", (HttpContext ctx) =>
{
    var filter = QueryStringBinder.Bind<BlogSearchFilter>(ctx);
    // filter.Title, filter.Status etc. are populated from query string
});
```

`QueryStringBinder` supports `string`, `int`, `bool`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, nullable variants of all the above, and `enum` types.

### OpenAPI configuration

```csharp
builder.Services.ConfigureOpenApiOptions(new OpenApiInfo
{
    Title = "My Blog API",
    Version = "v1"
});
```

## Configuration

The fluent builder accepts the following optional steps in order:

| Step | Method | Notes |
|---|---|---|
| AutoMapper profile | `WithAutoMapper<TProfile>()` | Registers `IMapper` with the given profile. |
| DbContext + repositories | `WithDbContext<TDbContext>(optionsAction?)` | Calls `AddDbContext` and `AddRepositories<TDbContext>()`. |
| Memory cache | `WithMemoryCache(configureOptions?)` | Adds `IMemoryCache` used by `GetByIdEndpointHandler`. |
| Map an entity type | `.MapEndpoints().MapType<TEntity, TId, TDto>(operations?)` | Registers all five handler services for the entity. Can be chained. |

`MapType` can be chained multiple times to register multiple entity types:

```csharp
.MapEndpoints()
.MapType<BlogPost, int, BlogPostDto>()
.MapType<Comment, int, CommentDto>()
.MapType<Tag, int, TagDto>()
```

## Related Libraries

- [Ploch.Common.Web](common-web.md) — Swagger configuration helpers for traditional controllers
- [Ploch.Common.AppServices](common-appservices.md) — `IUserInfoProvider` abstraction used in secured endpoints
- [Ploch.Common.AppServices.Web](common-appservices-web.md) — HTTP context implementation of `IUserInfoProvider`
