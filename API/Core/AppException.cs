using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Core;

/// <summary>
/// Represents a custom application exception.
/// Used to encapsulate errors with structured information (HTTP status code, message, and details).
/// Allows returning consistent and well-formatted error responses to the client.
/// </summary>
/// <param name="statusCode">HTTP status code (e.g., 400, 404, 500)</param>
/// <param name="message">Main user-friendly error message</param>
/// <param name="details">
/// Optional technical details about the error — may include stack trace or debug information.
/// </param>
public class AppException(int statusCode, string message, string? details)
{
    /// <summary>
    /// HTTP status code that indicates the type of error.
    /// Examples: 400 (Bad Request), 404 (Not Found), 500 (Internal Server Error).
    /// </summary>
    public int StatusCode { get; set; } = statusCode;

    /// <summary>
    /// Main error message.
    /// Should be clear and user-friendly.
    /// Example: "Animal not found".
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// Optional technical details about the error.
    /// May include stack trace, inner exception messages, or debug information.
    /// Usually only returned in development environments, not in production.
    /// Example: "NullReferenceException at line 42...".
    /// </summary>
    public string? Details { get; set; } = details;
}

