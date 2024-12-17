namespace albus.src;

public sealed class Result<T> 
    where T : notnull
{
    public readonly bool IsSuccess;
    public readonly string? Error;
    public readonly T? Value;
    
    private Result(bool isSuccess, string? error, T? token) {
        IsSuccess = isSuccess;
        Error = error;
        Value = token;
    }

    public static Result<T> Ok(T token) {
        return new(true, null, token);
    }
    public static Result<T> Err(string error) {
        return new(false, error, default);
    }
}