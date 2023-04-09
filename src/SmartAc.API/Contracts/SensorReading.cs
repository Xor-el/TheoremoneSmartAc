using SmartAc.Domain;

namespace SmartAc.API.Contracts;

public sealed record SensorReading(
    DateTimeOffset RecordedDateTime,
    decimal Temperature,
    decimal Humidity,
    decimal CarbonMonoxide,
    DeviceHealth Health)
{
    public DeviceReading ToDeviceReading(string serialNumber)
    {
        return new DeviceReading
        {
            DeviceSerialNumber = serialNumber,
            RecordedDateTime = RecordedDateTime,
            Temperature = Temperature,
            Humidity = Humidity,
            CarbonMonoxide = CarbonMonoxide,
            Health = Health,
        };
    }
}
