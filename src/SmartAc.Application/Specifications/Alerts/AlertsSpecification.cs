using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;

namespace SmartAc.Application.Specifications.Alerts
{
    internal class AlertsSpecification : BaseSpecification<Alert>
    {
        public AlertsSpecification(string deviceSerialNumber, AlertType alertType)
            : base(x => x.DeviceSerialNumber == deviceSerialNumber && x.AlertType == alertType)
        {
        }

        public AlertsSpecification(string deviceSerialNumber, AlertState alertState)
            : base(x => x.DeviceSerialNumber == deviceSerialNumber && x.AlertState == alertState)
        {
        }
    }
}
