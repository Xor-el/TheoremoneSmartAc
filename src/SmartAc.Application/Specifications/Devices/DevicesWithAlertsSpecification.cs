using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;

namespace SmartAc.Application.Specifications.Devices
{
    internal sealed class DevicesWithAlertsSpecification : BaseSpecification<Device>
    {
        public DevicesWithAlertsSpecification()
        {
            AddInclude(x => x.Alerts);
        }

        public DevicesWithAlertsSpecification(string serialNumber)
            : base(x => x.SerialNumber == serialNumber)
        {
            AddInclude(x => x.Alerts);
        }

        public DevicesWithAlertsSpecification(string serialNumber, AlertState alertState)
            : base(x => x.SerialNumber == serialNumber && x.Alerts.Any(a => a.AlertState == alertState))
        {
            AddInclude(x => x.Alerts);
        }
    }
}
