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
                reading.CarbonMonoxide.InRange(sensorParams.CarbonMonoxidePpmMin, sensorParams.CarbonMonoxidePpmMax) => true,

            AlertType.OutOfRangeHumidity when 
                reading.Humidity.InRange(sensorParams.HumidityPctMin, sensorParams.HumidityPctMax) => true,

            AlertType.DangerousCoLevel when 
                reading.CarbonMonoxide < sensorParams.CarbonMonoxideDangerLevel => true,

            AlertType.PoorHealth when 
                reading.Health == DeviceHealth.Ok => true,

            _ => false
        };
    }
}