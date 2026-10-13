namespace Ploch.Common.WebApi.Endpoints.Models;

/// <summary>
///     Represents a request with a data transfer object.
/// </summary>
/// <param name="data">The data transfer object.</param>
/// <typeparam name="TDataTransferObject">The type of data transfer object.</typeparam>
public class DataTransferObjectRequest<TDataTransferObject>(TDataTransferObject data)
{
    /// <summary>
    ///     Gets or sets the data transfer object associated with the request.
    /// </summary>
    public TDataTransferObject Data { get; set; } = data;
}
