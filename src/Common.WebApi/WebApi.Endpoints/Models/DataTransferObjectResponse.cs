namespace Ploch.Common.WebApi.Endpoints.Models;

/// <summary>
///     Represents a response object containing a data transfer object (DTO).
/// </summary>
/// <typeparam name="TDataTransferObject">The type of the data transfer object contained in the response.</typeparam>
public class DataTransferObjectResponse<TDataTransferObject>(TDataTransferObject data)
{
    /// <summary>
    ///     Gets or sets the data transfer object (DTO) associated with the response.
    /// </summary>
    /// <remarks>
    ///     Represents the primary data payload included in the response object. The type of the data
    ///     is defined by the generic parameter <typeparamref name="TDataTransferObject" />.
    /// </remarks>
    public TDataTransferObject Data { get; set; } = data;
}
