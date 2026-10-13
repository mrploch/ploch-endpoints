using Ardalis.Result;
using FluentAssertions;
using Ploch.Common.WebApi.Endpoints.Models;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetById;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.DTOs;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.GetById;
public class GetByIdEndpointHandlerTests : CrudEndpointsIntegrationTest
{
    [Fact]
    public async Task HandleAsync_should_return_item_with_specified_id()
    {
        var response = await GetById(5, 3);

        response.Status.Should().Be(ResultStatus.Ok);

        var data = response.Value.Data;
        data.Should().NotBeNull();
        data.Id.Should().Be(3);
        data.Name.Should().Be("Blog post 3");
    }

    [Fact]
    public async Task HandleAsync_should_return_NotFound_status_if_item_with_specified_id_is_not_null()
    {
        var response = await GetById(5, 10);

        response.Status.Should().Be(ResultStatus.NotFound);
    }

    private async Task<Result<DataTransferObjectResponse<BlogPostDto>>> GetById(int seedDataCount, int id)
    {
        await AddBlogPosts(seedDataCount);

        var request = new IdRequest<int> { Id = id };
        var sut = ServiceProvider.GetRequiredService<IGetByIdEndpointHandler<BlogPost, int, BlogPostDto>>();

        return await sut.HandleAsync(request, CancellationToken.None);
    }
}
