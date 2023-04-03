using ErrorOr;
using MediatR;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Contracts;
using SmartAc.Application.Specifications.Devices;
using SmartAc.Domain;

namespace SmartAc.Application.Features.Devices.GetAlertLogs;

internal sealed class GetAlertLogsQueryHandler : IRequestHandler<GetAlertLogsQuery, ErrorOr<IEnumerable<LogItem>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAlertLogsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ErrorOr<IEnumerable<LogItem>>> Handle(GetAlertLogsQuery request, CancellationToken cancellationToken)
    {
        IRepository<Device> repo = _unitOfWork.GetRepository<Device>();

        AlertState? alertState = request.Params.Filter switch
        {
            FilterType.New => AlertState.New,
            FilterType.Resolved => AlertState.Resolved,
            _ => null
        };

        var specification = alertState is null
            ? new DevicesWithAlertsSpecification(request.SerialNumber)
            : new DevicesWithAlertsSpecification(request.SerialNumber, alertState.Value);

        if (!await repo.ContainsAsync(specification, cancellationToken))
        {
            return Error.NotFound(
                "Device.NotFound",
                $"Device with serial number '{request.SerialNumber}' was not found");
        }

        var device = repo
            .Find(specification)
            .Single();

        var joinQuery =
            device.Alerts.GroupJoin(device.DeviceReadings,
                alert => alert.Device,
                reading => reading.Device,
                (alert, readings) => new LogItem
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