using Microsoft.Extensions.DependencyInjection;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.DTOs;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Data;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests;

public class CrudEndpointsIntegrationTest : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddAutoMapper(typeof(EntityToDtoProfile))
                .AddLogging()
                .AddCrudEndpoints()
                .WithAutoMapper<EntityToDtoProfile>()
                .MapEndpoints()
                .MapType<BlogPost, int, BlogPostDto>();
    }

    protected async Task<IList<BlogPost>> AddBlogPosts(int count)
    {
        var data = EntitiesBuilder.BuildBlogPosts(count);
        await DbContext.BlogPosts.AddRangeAsync(data);
        await DbContext.SaveChangesAsync();

        return data.ToList();
    }
}
