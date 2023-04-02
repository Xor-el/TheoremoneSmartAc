using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAc.Domain;

public class DeviceReading : EntityBase
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DeviceReadingId { get; private set; }


    [Column(TypeName = "decimal(5, 2)")]
    public decimal Temperature { get; set; }


    [Column(TypeName = "decimal(5, 2)")]
    public decimal Humidity { get; set; }


    [Column(TypeName = "decimal(5, 2)")]
    public decimal CarbonMonoxide { get; set; }

    public DeviceHealth Health { get; set; }

    public DateTimeOffset RecordedDateTime { get; set; }

    public DateTimeOffset ReceivedDateTime { get; private set; } = DateTimeOffset.UtcNow;

    public string DeviceSerialNumber { get; set; } = string.Empty;

    public Device Device { get; private set; } = null!;
}