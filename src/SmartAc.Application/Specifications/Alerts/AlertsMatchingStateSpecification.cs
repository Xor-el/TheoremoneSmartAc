using System.Linq.Expressions;
using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;

namespace SmartAc.Application.Specifications.Alerts;

internal sealed class AlertsMatchingStateSpecification : BaseSpecification<Alert>
{
    public AlertsMatchingStateSpecification(string deviceSerialNumber, AlertState alertState, int[] idsToExclude)
        : base(x =>
            x.DeviceSerialNumber == deviceSerialNumber &&
            x.AlertState == alertState &&
            !idsToExclude.Contains(x.AlertId))
    {
        ApplyOrderBy(x => x.ReportedDateTime);
    }

    public AlertsMatchingStateSpecification(Expression<Func<Alert, bool>> predicate) : base(predicate)
    {
        ApplyOrderBy(x => x.ReportedDateTime);
    }
}