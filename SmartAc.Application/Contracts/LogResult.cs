using SmartAc.Domain;

namespace SmartAc.Application.Contracts;

public sealed record LogResult
{
    public AlertType AlertType { get; init; }

    public string Message { get; init; } = string.Empty;

    public decimal MinValue { get; init; }

    public decimal MaxValue { get; init; }

    public AlertState AlertState { get; init; }

    public DateTimeOffset DateTimeCreated { get; init; }

    public DateTimeOffset DateTimeReported { get; init; }

    public DateTimeOffset? DateTimeLastReported { get; init; }

}