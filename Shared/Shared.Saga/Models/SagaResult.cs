namespace Shared.Saga.Models;

public class SagaResult
{
    public bool IsSuccess { get; private set; }
    public SagaStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Exception? Exception { get; private set; }
    public int LastCompletedStep { get; private set; }
    public Dictionary<string, object> Data { get; private set; } = new();

    private SagaResult(bool isSuccess, SagaStatus status, int lastCompletedStep = -1, string? errorMessage = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Status = status;
        LastCompletedStep = lastCompletedStep;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static SagaResult Success() => new(true, SagaStatus.Completed);

    public static SagaResult Success(Dictionary<string, object> data)
    {
        var result = new SagaResult(true, SagaStatus.Completed);
        result.Data = data;
        return result;
    }

    public static SagaResult Failed(string errorMessage, int lastCompletedStep) => 
        new(false, SagaStatus.Failed, lastCompletedStep, errorMessage);

    public static SagaResult Failed(string errorMessage, int lastCompletedStep, Exception exception) => 
        new(false, SagaStatus.Failed, lastCompletedStep, errorMessage, exception);

    public static SagaResult Compensated() => new(false, SagaStatus.Compensated);

    public static SagaResult CompensationFailed(string errorMessage, Exception? exception = null) => 
        new(false, SagaStatus.CompensationFailed, errorMessage: errorMessage, exception: exception);

    public SagaResult WithData(string key, object value)
    {
        Data[key] = value;
        return this;
    }
}
