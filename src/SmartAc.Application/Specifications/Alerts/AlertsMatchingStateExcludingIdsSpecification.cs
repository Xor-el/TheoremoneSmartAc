using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;

namespace SmartAc.Application.Specifications.Alerts;

internal sealed class AlertsMatchingStateExcludingIdsSpecification : BaseSpecification<Alert>
{
    public AlertsMatchingStateExcludingIdsSpecification(string deviceSerialNumber, AlertState alertState, int[] idsToExclude)
        :base(x => 
            x.DeviceSerialNumber == deviceSerialNumber && 
            x.AlertState == alertState &&
            !idsToExclude.Contains(x.AlertId))
    {
        ApplyOrderBy(x => x.ReportedDateTime);
    }
}