using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAc.Domain;

public class Alert : EntityBase
{
    private Alert(){}

    private Alert(AlertType alertType, string deviceSerialNumber, DateTimeOffset reportedDateTime, string message)
    {
        DeviceSerialNumber = deviceSerialNumber;
        DateTimeReported = reportedDateTime;
        Message = message;
        AlertType = alertType;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AlertId { get; private set; }

    public AlertType AlertType { get; private set; }


    [Required, MaxLength(200)]
    public string Message { get; private set; }

    public AlertState AlertState { get; private set; } = AlertState.New;

    public DateTimeOffset DateTimeCreated { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset DateTimeReported { get; private set; }

    public DateTimeOffset? DateTimeLastReported { get; private set; }

    public string DeviceSerialNumber { get; private set; }

    public Device Device { get; private set; } = null!;

    public static Alert CreateNew(
        AlertType alertType,
        string deviceSerialNumber,
        DateTimeOffset reportDate,
        string alertMessage)
    {
        return new Alert(alertType, deviceSerialNumber, reportDate, alertMessage);
    }

    public void Update(DateTimeOffset alertLastReportedDateTime, string message, AlertState alertState)
    {
        Message = message;
        AlertState = alertState;
        DateTimeLastReported = alertLastReportedDateTime;
    }
}