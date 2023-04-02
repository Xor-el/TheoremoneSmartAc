using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;

namespace SmartAc.Application.Specifications.Devices
{
    internal sealed class DevicesWithRegistrationsSpecification : BaseSpecification<Device>
    {
        public DevicesWithRegistrationsSpecification()
        {
            AddInclude(x => x.DeviceRegistrations);
        }

        public DevicesWithRegistrationsSpecification(string serialNumber)
            : base(x => x.SerialNumber == serialNumber)
        {
            AddInclude(x => x.DeviceRegistrations);
        }

        public DevicesWithRegistrationsSpecification(string serialNumber, string sharedSecret)
            : base(x => x.SerialNumber == serialNumber && x.SharedSecret == sharedSecret)
        {
            AddInclude(x => x.DeviceRegistrations);
        }
    }
}
