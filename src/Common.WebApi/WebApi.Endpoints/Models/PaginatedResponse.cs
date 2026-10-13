namespace Ploch.Common.WebApi.Endpoints.Models;

public class PaginatedResponse<TDataTransferObject>(IEnumerable<TDataTransferObject> items, int pageNumber, int pageSize, int? totalItems = null, bool? moreItems = null)
{
    public IEnumerable<TDataTransferObject> Items { get; set; } = items;

    public int PageNumber { get; set; } = pageNumber;

    public int PageSize { get; set; } = pageSize;

    public int? TotalItems { get; set; } = totalItems;

    public bool? MoreItems { get; set; } = moreItems;
}
