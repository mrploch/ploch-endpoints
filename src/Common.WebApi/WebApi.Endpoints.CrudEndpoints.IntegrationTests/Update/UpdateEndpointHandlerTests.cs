using Ardalis.Result;
using FluentAssertions;
using Ploch.Common.WebApi.Endpoints.Models;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.Update;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.DTOs;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.Update;
public class UpdateEndpointHandlerTests : CrudEndpointsIntegrationTest
{
    [Fact]
    public async Task HandleAsync_should_update_specified_entity()
    {
        await AddBlogPosts(10);

        var sut = ServiceProvider.GetRequiredService<IUpdateEndpointHandler<BlogPost, int, BlogPostDto>>();

        var request = new DataTransferObjectRequest<BlogPostDto>(new BlogPostDto { Id = 5, Name = "Updated blog post" });

        var result = await sut.HandleAsync(request, CancellationToken.None);
        result.Status.Should().Be(ResultStatus.NoContent);

        var repository = CreateReadRepositoryAsync<BlogPost, int>();
        for (var i = 1; i <= 10; i++)
        {
            var entity = await repository.GetByIdAsync(i);
            entity.Name.Should().Be(i == 5 ? "Updated blog post" : $"Blog post {i}");
        }
    }

    [Fact]
    public async Task HandleAsync_should_return_NotFound_if_updated_entity_is_not_found()
    {
        await AddBlogPosts(5);

        var sut = ServiceProvider.GetRequiredService<IUpdateEndpointHandler<BlogPost, int, BlogPostDto>>();

        var request = new DataTransferObjectRequest<BlogPostDto>(new BlogPostDto { Id = 7, Name = "Updated blog post" });

        var result = await sut.HandleAsync(request, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
