using ErrorOr;
using MediatR;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Contracts;
using SmartAc.Application.Specifications.Devices;
using SmartAc.Domain;

namespace SmartAc.Application.Features.Devices.GetAlertLogs;

internal sealed class GetAlertLogsQueryHandler : IRequestHandler<GetAlertLogsQuery, ErrorOr<IEnumerable<LogResult>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAlertLogsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ErrorOr<IEnumerable<LogResult>>> Handle(GetAlertLogsQuery request, CancellationToken cancellationToken)
    {
        IRepository<Device> repo = _unitOfWork.GetRepository<Device>();

        var alertState = request.FilterType switch
        {
            FilterType.New => AlertState.New,
            FilterType.Resolved => AlertState.Resolved,
            _ => AlertState.New | AlertState.Resolved
        };

        var specification = new DevicesWithAlertsSpecification(request.SerialNumber, alertState);

        if (!await repo.ContainsAsync(specification, cancellationToken))
        {
            return Error.NotFound(
                "Device.NotFound",
                $"Device with serial number '{request.SerialNumber}' not found");
        }

        Device device = repo.Find(specification).Single();

        var joinQuery =
            device.Alerts.GroupJoin(device.DeviceReadings,
                alert => alert.Device,
                reading => reading.Device,
                (alert, readings) => new LogResult
                {
                    AlertType = alert.AlertType,
                    Message = alert.Message,
                    AlertState = alert.AlertState,
                    DateTimeCreated = alert.DateTimeCreated,
                    DateTimeReported = alert.DateTimeReported,
                    DateTimeLastReported = alert.DateTimeLastReported,
                    MinValue = alert.AlertType switch
                    {
                        AlertType.OutOfRangeTemp => readings.Min(x => x.Temperature),
                        AlertType.OutOfRangeCo => readings.Min(x => x.CarbonMonoxide),
                        AlertType.OutOfRangeHumidity => readings.Min(x => x.Humidity),
                        AlertType.DangerousCoLevel => readings.Min(x => x.CarbonMonoxide),
                        _ => 0m
                    },
                    MaxValue = alert.AlertType switch
                    {
                        AlertType.OutOfRangeTemp => readings.Max(x => x.Temperature),
                        AlertType.OutOfRangeCo => readings.Max(x => x.CarbonMonoxide),
                        AlertType.OutOfRangeHumidity => readings.Max(x => x.Humidity),
                        AlertType.DangerousCoLevel => readings.Max(x => x.CarbonMonoxide),
                        _ => 0m
                    },
                });

        return joinQuery.ToList();
    }
}