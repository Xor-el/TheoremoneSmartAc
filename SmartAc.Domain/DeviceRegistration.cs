using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartAc.Domain;

public class DeviceRegistration : EntityBase
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DeviceRegistrationId { get; set; }

    public Device Device { get; private set; } = null!;

    public string DeviceSerialNumber { get; set; } = string.Empty;

    public DateTimeOffset RegistrationDate { get; set; } = DateTimeOffset.Now;

    public string TokenId { get; set; } = string.Empty;

    public bool Active { get; set; } = true;
}