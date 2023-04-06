using SmartAc.Application.Options;
using SmartAc.Domain;

namespace SmartAc.Application.Extensions;

public static class DeviceReadingExtensions
{
    public static IEnumerable<Alert> GetPotentialAlerts(this DeviceReading reading, SensorParams sensorParams)
    {
        if (!reading.Temperature.InRange(sensorParams.TemperatureMin, sensorParams.TemperatureMax))
        {
            yield return Alert.CreateNew(
                AlertType.OutOfRangeTemp,
                reading.DeviceSerialNumber,
                reading.RecordedDateTime,
                $"Sensor {reading.DeviceSerialNumber} reported out-of-range Temperature");
        }

        if (!reading.CarbonMonoxide.InRange(sensorParams.CarbonMonoxidePpmMin, sensorParams.CarbonMonoxidePpmMax))
        {
            yield return Alert.CreateNew(
                AlertType.OutOfRangeCo,
                reading.DeviceSerialNumber,
                reading.RecordedDateTime,
                $"Sensor {reading.DeviceSerialNumber} reported out-of-range carbon Monoxide levels");
        }

        if (reading.CarbonMonoxide >= sensorParams.CarbonMonoxideDangerLevel)
        {
            yield return Alert.CreateNew(
                AlertType.DangerousCoLevel,
                reading.DeviceSerialNumber,
                reading.RecordedDateTime,
                $"Sensor {reading.DeviceSerialNumber} - Reported CO value has exceeded danger limit");
        }

        if (!reading.Humidity.InRange(sensorParams.HumidityPctMin, sensorParams.HumidityPctMax))
        {
            yield return Alert.CreateNew(
                AlertType.OutOfRangeHumidity,
                reading.DeviceSerialNumber,
                reading.RecordedDateTime,
                $"Sensor {reading.DeviceSerialNumber} reported out-of-range humidity levels");
        }

        if (reading.Health != DeviceHealth.Ok)
        {
            yield return Alert.CreateNew(
                AlertType.PoorHealth,
                reading.DeviceSerialNumber,
                reading.RecordedDateTime,
                $"Sensor {reading.DeviceSerialNumber} is reporting health problem: {reading.Health}");
        }
    }
}