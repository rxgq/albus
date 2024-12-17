namespace albus.src;

public sealed class Result<T>
    where T : notnull
{
    public readonly bool IsSuccess;
    public readonly T? Value;
    
    private Result(bool isSuccess, T? token) {
        IsSuccess = isSuccess;
        Value = token;
    }

    public static Result<T> Ok(T token) {
        return new(true, token);
    }
    public static Result<T> Err() {
        return new(false, default);
    }
}