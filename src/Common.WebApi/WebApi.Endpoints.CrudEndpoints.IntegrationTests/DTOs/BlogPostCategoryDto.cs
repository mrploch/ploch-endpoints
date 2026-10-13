namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.DTOs;

public class BlogPostCategoryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ParentId { get; set; }
}