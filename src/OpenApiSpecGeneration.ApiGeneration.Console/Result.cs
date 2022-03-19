namespace OpenApiSpecGeneration.Console;

public record Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }

    private Result(bool success, T? value) => (IsSuccess, Value) = (success, value);

    public static Result<T> Success(T value) => new Result<T>(true, value);
    public static Result<T> Failure() => new Result<T>(false, default);
}
