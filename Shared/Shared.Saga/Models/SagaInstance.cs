using System.Text.Json;

namespace Shared.Saga.Models;

public class SagaInstance
{
    public Guid Id { get; set; }
    public string SagaType { get; set; } = string.Empty;
    public SagaStatus Status { get; set; }
    public int CurrentStep { get; set; }
    public int LastCompletedStep { get; set; }
    public string Data { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;

    public T GetData<T>() where T : class
    {
        if (string.IsNullOrEmpty(Data))
            throw new InvalidOperationException("Saga data is empty");

        var result = JsonSerializer.Deserialize<T>(Data);
        return result ?? throw new InvalidOperationException("Failed to deserialize saga data");
    }

    public void SetData<T>(T data) where T : class
    {
        Data = JsonSerializer.Serialize(data);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(SagaStatus status, string? errorMessage = null)
    {
        Status = status;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdvanceStep()
    {
        LastCompletedStep = CurrentStep;
        CurrentStep++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCurrentStep(int step)
    {
        CurrentStep = step;
        UpdatedAt = DateTime.UtcNow;
    }
}
