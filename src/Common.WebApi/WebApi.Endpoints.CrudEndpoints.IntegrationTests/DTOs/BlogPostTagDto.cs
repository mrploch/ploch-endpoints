namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.DTOs;

public class BlogPostTagDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}