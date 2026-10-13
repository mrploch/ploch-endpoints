namespace Ploch.Common.WebApi.Endpoints.Models;

/// <summary>
///     Represents a request for a page of items.
/// </summary>
public class PaginatedRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
