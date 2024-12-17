namespace albus.src;

public sealed class Result<T> 
    where T : class
{
    public readonly string? Error;
    public readonly T? Value;
    
    private Result(string? error, T? token) {
        Error = error;
        Value = token;
    }

    public static Result<T> Ok(T token) {
        return new(null, token);
    }
    public static Result<T> Err(string error) {
        return new(error, default);
    }
}