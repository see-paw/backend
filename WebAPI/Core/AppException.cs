
namespace WebAPI.Core;

/// <summary>
/// Represents a standardized application-level exception response.
/// </summary>
/// <remarks>
/// Used to return structured error information in API responses, including
/// the HTTP status code, message, and optional technical details for debugging.
/// </remarks>
public class AppException(int statusCode, string message, string? details)
{
    public int StatusCode { get; set; } = statusCode;

    public string Message { get; set; } = message;
    public string? Details { get; set; } = details;
}