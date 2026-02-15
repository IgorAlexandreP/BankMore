namespace BankMore.Core.Shared;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public string ErrorType { get; }

    protected Result(bool isSuccess, string error, string errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string errorType) => new(false, error, errorType);
}

public class Result<T> : Result
{
    public T Value { get; }

    protected Result(T value, bool isSuccess, string error, string errorType) 
        : base(isSuccess, error, errorType)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, null, null);
    public new static Result<T> Failure(string error, string errorType) => new(default, false, error, errorType);
}
