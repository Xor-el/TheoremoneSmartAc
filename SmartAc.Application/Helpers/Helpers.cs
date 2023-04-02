using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Domain;

namespace SmartAc.Application.Helpers;

public static class Helpers
{
    public static bool IsResolved(AlertType alertType, DeviceReading reading, SensorParams sensorParams)
    {
        return alertType switch
        {
            AlertType.OutOfRangeTemp when 
                reading.Temperature.InRange(sensorParams.TemperatureMin, sensorParams.TemperatureMax) => true,

            AlertType.OutOfRangeCo when 
                reading.CarbonMonoxide.InRange(sensorParams.CarbonMonoxideMin, sensorParams.CarbonMonoxideMax) => true,

            AlertType.OutOfRangeHumidity when 
                reading.Humidity.InRange(sensorParams.HumidityMin, sensorParams.HumidityMax) => true,

            AlertType.DangerousCoLevel when 
                reading.CarbonMonoxide < sensorParams.CarbonMonoxideThreshold => true,

            AlertType.PoorHealth when 
                reading.Health == DeviceHealth.Ok => true,

            _ => false
        };
    }
}