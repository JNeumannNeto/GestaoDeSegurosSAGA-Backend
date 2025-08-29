namespace Shared.Saga.Models;

public class StepResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Exception? Exception { get; private set; }
    public Dictionary<string, object> Data { get; private set; } = new();

    private StepResult(bool isSuccess, string? errorMessage = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static StepResult Success() => new(true);

    public static StepResult Success(Dictionary<string, object> data)
    {
        var result = new StepResult(true);
        result.Data = data;
        return result;
    }

    public static StepResult Failed(string errorMessage) => new(false, errorMessage);

    public static StepResult Failed(string errorMessage, Exception exception) => new(false, errorMessage, exception);

    public StepResult WithData(string key, object value)
    {
        Data[key] = value;
        return this;
    }
}
