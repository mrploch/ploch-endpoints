namespace Ploch.Common.WebApi.Endpoints.Models;

/// <summary>
///     A generic request structure that encapsulates an identifier. It is commonly used for API endpoints that operate
///     on a specific entity identified by its ID.
/// </summary>
/// <typeparam name="TId">
///     The type of the identifier. This could be an integer, GUID, string, or any other type used for entity
///     identification.
/// </typeparam>
public class IdRequest<TId>
{
    /// <summary>
    ///     Represents the unique identifier of the entity or object used for processing.
    ///     This property is a generic type, allowing flexibility to support various types of identifiers.
    /// </summary>
    public TId Id { get; set; } = default!;
}
