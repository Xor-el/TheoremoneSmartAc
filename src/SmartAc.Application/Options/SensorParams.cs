namespace SmartAc.Application.Options;

public sealed class SensorParams
{
    public decimal TemperatureMin { get; init; }
    public decimal TemperatureMax { get; init; }
    public decimal HumidityPctMin { get; init; }
    public decimal HumidityPctMax { get; init; }
    public decimal CarbonMonoxidePpmMin { get; init; }
    public decimal CarbonMonoxidePpmMax { get; init; }
    public decimal CarbonMonoxideDangerLevel { get; init; }
    public int ReadingTimespanMinutes { get; init; } = 15;

}