namespace SmartAc.Application.Options;

public sealed class SensorParams
{
    public decimal TemperatureMin { get; init; }
    public decimal TemperatureMax { get; init; }
    public decimal HumidityMin { get; init; }
    public decimal HumidityMax { get; init; }
    public decimal CarbonMonoxideMin { get; init; }
    public decimal CarbonMonoxideMax { get; init; }
    public decimal CarbonMonoxideThreshold { get; init; }
    public int ReadingTimespanMinutes { get; init; } = 15;

}