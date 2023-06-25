public class Result<TSuccess, TError>
{
    public bool IsSuccess { get; }
    public TSuccess SuccessValue { get; set; }
    public TError ErrorValue { get; }

    private Result() => IsSuccess = true;
    private Result(TError errorValue) => (IsSuccess, ErrorValue) = (false, errorValue);

    public static Result<TSuccess, TError> Ok(TSuccess value) => new Result<TSuccess, TError> { SuccessValue = value };
    public static Result<TSuccess, TError> Error(TError errorValue) => new Result<TSuccess, TError>(errorValue);
}