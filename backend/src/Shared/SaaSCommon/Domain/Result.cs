namespace SaaSCommon.Domain;

/// <summary>
/// A generic result monad for encapsulating operation outcomes.
/// </summary>
public readonly record struct Result<T>
{
    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    private Result(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    public static Result<T> Success(T value) => new(value, null);

    /// <summary>
    /// Creates a failed result with the given error message.
    /// </summary>
    public static Result<T> Failure(string error) => new(default, error);
}

/// <summary>
/// Non-generic result for operations that don't return a value.
/// </summary>
public readonly record struct Result
{
    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    private Result(string? error) => Error = error;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(null);

    /// <summary>
    /// Creates a failed result with the given error message.
    /// </summary>
    public static Result Failure(string error) => new(error);
}
