namespace Application.Core;

/// <summary>
/// Represents a standardized result object used to encapsulate operation outcomes.
/// </summary>
/// <typeparam name="T">The type of the data returned by the operation.</typeparam>
/// <remarks>
/// Provides a unified way to represent both successful and failed results across the application layer.  
/// Includes fields for success state, returned value, error message, and HTTP status code.
/// </remarks>
public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }

    public string? Error { get; init; }
    public int Code { get; init; }

    /// <summary>
    /// Creates a successful result containing the provided value and status code.
    /// </summary>
    /// <param name="value">The data returned by the successful operation.</param>
    /// <param name="code">The HTTP status code associated with the successful result.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> instance representing a successful operation,
    /// containing the specified value and status code.
    /// </returns>
    /// <remarks>
    /// Used to standardize success responses across the application layer,
    /// ensuring consistency in API result handling.
    /// </remarks>
    public static Result<T> Success(T value, int code) => new()
    {
        IsSuccess = true, 
        Value = value,
        Code = code
    };

    /// <summary>
    /// Creates a failed result containing an error message and status code.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="code">The HTTP status code associated with the failure.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> instance representing a failed operation,
    /// containing the specified error message and status code.
    /// </returns>
    /// <remarks>
    /// Used to standardize error responses across the application layer,
    /// ensuring consistent handling of failed operations.
    /// </remarks>
    public static Result<T> Failure(string error, int code) => new()
    {
        IsSuccess = false,
        Error = error,
        Code = code
    };
}
