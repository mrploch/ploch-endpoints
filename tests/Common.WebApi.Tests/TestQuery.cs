namespace Ploch.Common.WebApi.Tests;

/// <summary>
///     Query model covering every property type <see cref="QueryStringBinder" /> supports.
/// </summary>
public class TestQuery
{
    public string? Name { get; set; }

    public int Page { get; set; }

    public int? OptionalPage { get; set; }

    public bool IncludeArchived { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }

    public DateOnly StartDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public SortDirection Direction { get; set; }
}

/// <summary>
///     Sort direction used to exercise enum binding.
/// </summary>
public enum SortDirection
{
    Ascending,
    Descending
}

/// <summary>
///     Query model with a property type the binder does not support.
/// </summary>
public class UnsupportedQuery
{
    public Uri? Endpoint { get; set; }
}
