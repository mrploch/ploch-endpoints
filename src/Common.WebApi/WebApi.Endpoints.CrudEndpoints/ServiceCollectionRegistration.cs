using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Create;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Delete;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetAll;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetById;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Update;
using Ploch.Data.GenericRepository.EFCore;
using Ploch.Data.Model;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints;

public static class ServiceCollectionRegistration
{
    public static CrudEndpointsBuilder AddCrudEndpoints(this IServiceCollection services) => new(services);
}

public class CrudEndpointsBuilder(IServiceCollection services)
{
    public CrudEndpointsBuilder WithAutoMapper<TAutoMapperProfile>()
    {
        services.AddAutoMapper(typeof(TAutoMapperProfile));

        return this;
    }

    public CrudEndpointsBuilder WithDbContext<TDbContext>(Action<DbContextOptionsBuilder>? optionsAction = null)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>(optionsAction)
                .AddRepositories<TDbContext>();

        return this;
    }

    public CrudEndpointsBuilder WithMemoryCache(Action<MemoryCacheOptions>? configureOptions = null)
    {
        if (configureOptions == null)
        {
            services.AddMemoryCache();
        }
        else
        {
            services.AddMemoryCache(configureOptions);
        }

        return this;
    }

    public EntityTypeEndpointMapper MapEndpoints() => new(services);
}

public class EntityTypeEndpointMapper(IServiceCollection services)
{
    public EntityTypeEndpointMapper MapType<TEntity, TId, TDto>(EntityQueryOperations<TEntity>? operations = null)
        where TEntity : class, IHasId<TId>
    {
        operations ??= new EntityQueryOperations<TEntity>();
        services.AddSingleton(operations);
        services.AddScoped<IGetListEndpointHandler<TEntity, TId, TDto>, GetListEndpointHandler<TEntity, TId, TDto>>()
                .AddScoped<IGetByIdEndpointHandler<TEntity, TId, TDto>, GetByIdEndpointHandler<TEntity, TId, TDto>>()
                .AddScoped<IUpdateEndpointHandler<TEntity, TId, TDto>, UpdateEndpointHandler<TEntity, TId, TDto>>()
                .AddScoped<IDeleteEndpointHandler<TEntity, TId>, DeleteEndpointHandler<TEntity, TId>>()
                .AddScoped<ICreateEndpointHandler<TEntity, TId, TDto>, CreateEndpointHandler<TEntity, TId, TDto>>();

        return this;
    }
}
