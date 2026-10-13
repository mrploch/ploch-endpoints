namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.DTOs;

public class BlogPostDto
{
    public int Id { get; set; }

    public string? Contents { get; set; }

    public string Name { get; set; } = default!;

    public DateTimeOffset? CreatedTime { get; set; }

    public DateTimeOffset? ModifiedTime { get; set; }

    public IEnumerable<BlogPostTagDto>? Tags { get; set; }

    public IEnumerable<BlogPostCategoryDto>? Categories { get; set; }
}