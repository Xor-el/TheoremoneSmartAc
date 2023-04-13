using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;

namespace SmartAc.Application.Specifications.Devices
{
    internal sealed class DevicesWithAlertsSpecification : BaseSpecification<Device>
    {
        public DevicesWithAlertsSpecification()
        {
            AddInclude(x => x.Alerts);
            AddInclude(x => x.DeviceReadings);
        }

        public DevicesWithAlertsSpecification(string serialNumber)
            : base(x => x.SerialNumber == serialNumber)
        {
            AddInclude(x => x.Alerts);
            AddInclude(x => x.DeviceReadings);
        }

        public DevicesWithAlertsSpecification(string serialNumber, int skip, int take)
            : base(x => x.SerialNumber == serialNumber)
        {
            AddInclude(x => x.Alerts.Skip(skip).Take(take).OrderBy(a => a.AlertId));
            AddInclude(x => x.DeviceReadings);
        }

        public DevicesWithAlertsSpecification(string serialNumber, AlertState alertState)
            : base(x => x.SerialNumber == serialNumber)
        {
            AddInclude(x => x.Alerts.Where(a => a.AlertState == alertState));
            AddInclude(x => x.DeviceReadings);
        }

        public DevicesWithAlertsSpecification(string serialNumber, AlertState alertState, int skip, int take)
            : base(x => x.SerialNumber == serialNumber)
        {
            AddInclude(x => x.Alerts.Where(a => a.AlertState == alertState).Skip(skip).Take(take).OrderBy(a => a.AlertId));
            AddInclude(x => x.DeviceReadings);
        }
    }
}
