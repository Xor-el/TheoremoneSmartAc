using System.Text.Json.Serialization;
using SmartAc.Domain;

namespace SmartAc.Application.Contracts;

public sealed record LogItem
{
    public AlertType AlertType { get; init; }

    public string Message { get; init; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal MinValue { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal MaxValue { get; init; }

    public AlertState AlertState { get; init; }

    public DateTimeOffset DateTimeCreated { get; init; }

    public DateTimeOffset DateTimeReported { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? DateTimeLastReported { get; init; }

}