
namespace WebAPI.Core;

/// <summary>
/// Represents a standardized application-level exception response.
/// </summary>
/// <remarks>
/// Used to return structured error information in API responses, including
/// the HTTP status code, message, and optional technical details for debugging.
/// </remarks>
public class AppException
{
    /// <summary>
    /// The HTTP status code associated with the exception.
    /// </summary>
    public required int StatusCode { get; set; }

    /// <summary>
    /// A short message describing the error.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Optional technical details about the exception for debugging purposes.
    /// </summary>
    public string? Details { get; set; }

}