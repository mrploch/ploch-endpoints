using FluentAssertions;
using Ploch.Common.WebApi.Endpoints.Models;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.GetAll;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.DTOs;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.GetAll;
public class GetListEndpointHandlerTests : CrudEndpointsIntegrationTest
{
    [Fact]
    public async Task HandleAsync_should_return_all_items_pages_with_total_count()
    {
        await AddBlogPosts(100);

        var sut = ServiceProvider.GetRequiredService<IGetListEndpointHandler<BlogPost, int, BlogPostDto>>();

        var pageOneResponse = await sut.HandleAsync(new PaginatedRequest { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
        var pageTwoResponse = await sut.HandleAsync(new PaginatedRequest { PageNumber = 2, PageSize = 20 }, CancellationToken.None);

        pageOneResponse.Value.PageNumber.Should().Be(1);
        pageOneResponse.Value.PageSize.Should().Be(10);
        pageOneResponse.Value.TotalItems.Should().Be(100);
        pageOneResponse.Value.Items.Should().HaveCount(10);

        ValidateItems(pageOneResponse.Value.Items, 1, "Blog post 1", 10, "Blog post 10");

        pageTwoResponse.Value.PageNumber.Should().Be(2);
        pageTwoResponse.Value.PageSize.Should().Be(20);
        pageTwoResponse.Value.TotalItems.Should().Be(100);
        pageTwoResponse.Value.Items.Should().HaveCount(20);

        ValidateItems(pageTwoResponse.Value.Items, 21, "Blog post 21", 40, "Blog post 40");
    }

    [Fact]
    public async Task HandleAsync_should_return_something_when_out_of_range()
    {
        await AddBlogPosts(10);

        var sut = ServiceProvider.GetRequiredService<IGetListEndpointHandler<BlogPost, int, BlogPostDto>>();

        try
        {
            var pageTwoResponse = await sut.HandleAsync(new PaginatedRequest { PageNumber = 2, PageSize = 20 }, CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            throw;
        }
    }

    private static void ValidateItems(IEnumerable<BlogPostDto> posts, int firstId, string firstName, int lastId, string lastName)
    {
        var first = posts.First();
        first.Id.Should().Be(firstId);
        first.Name.Should().Be(firstName);

        var last = posts.Last();

        last.Id.Should().Be(lastId);
        last.Name.Should().Be(lastName);
    }
}
