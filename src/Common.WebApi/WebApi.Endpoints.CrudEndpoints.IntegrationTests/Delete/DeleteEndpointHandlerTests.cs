using Ardalis.Result;
using FluentAssertions;
using Ploch.Common.WebApi.Endpoints.Models;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Delete;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.Delete;
public class DeleteEndpointHandlerTests : CrudEndpointsIntegrationTest
{
    [Fact]
    public async Task HandleAsync_should_delete_entity_if_exist()
    {
        await AddBlogPosts(5);

        var sut = ServiceProvider.GetRequiredService<IDeleteEndpointHandler<BlogPost, int>>();

        var result = await sut.HandleAsync(new IdRequest<int> { Id = 3 }, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.Ok);

        var repository = CreateReadRepositoryAsync<BlogPost, int>();
        for (var i = 1; i <= 5; i++)
        {
            var entity = await repository.GetByIdAsync(i);
            if (i == 3)
            {
                entity.Should().BeNull();
            }
            else
            {
                entity.Should().NotBeNull();
            }
        }
    }

    [Fact]
    public async Task HandleAsync_should_return_not_found_result_if_entity_is_not_found()
    {
        await AddBlogPosts(5);
        var sut = ServiceProvider.GetRequiredService<IDeleteEndpointHandler<BlogPost, int>>();

        var result = await sut.HandleAsync(new IdRequest<int> { Id = 10 }, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
