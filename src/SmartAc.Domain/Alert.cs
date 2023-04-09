using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAc.Domain;

public class Alert : EntityBase
{
#pragma warning disable CS8618
    private Alert(){}
#pragma warning restore CS8618

    private Alert(AlertType alertType, string deviceSerialNumber, DateTimeOffset reportedDateTime, string message)
    {
        DeviceSerialNumber = deviceSerialNumber;
        ReportedDateTime = reportedDateTime;
        Message = message;
        AlertType = alertType;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AlertId { get; private set; }

    public AlertType AlertType { get; private set; }


    [Required, MaxLength(200)]
    public string Message { get; private set; }

    public AlertState AlertState { get; private set; } = AlertState.New;

    public DateTimeOffset CreatedDateTime { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ReportedDateTime { get; private set; }

    public DateTimeOffset? LastReportedDateTime { get; private set; }

    public string DeviceSerialNumber { get; private set; }

    public static Alert CreateNew(AlertType alertType, string deviceSerialNumber, DateTimeOffset reportDate, string alertMessage)
    {
        return new Alert(alertType, deviceSerialNumber, reportDate, alertMessage);
    }

    public void Update(DateTimeOffset lastReportedDateTime, string message, AlertState alertState)
    {
        LastReportedDateTime = lastReportedDateTime;
        Message = message;
        AlertState = alertState;
    }
}