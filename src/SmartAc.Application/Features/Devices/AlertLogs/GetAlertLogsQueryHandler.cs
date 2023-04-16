using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Contracts;
using SmartAc.Application.Specifications.Devices;
using SmartAc.Domain;

namespace SmartAc.Application.Features.Devices.AlertLogs;

internal sealed class GetAlertLogsQueryHandler : IRequestHandler<GetAlertLogsQuery, ErrorOr<PagedList<LogItem>>>
{
    private readonly IRepository<Device> _repository;

    public GetAlertLogsQueryHandler(IRepository<Device> repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<PagedList<LogItem>>> Handle(
        GetAlertLogsQuery request,
        CancellationToken cancellationToken)
    {
        AlertState? alertState = request.Params.Filter switch
        {
            FilterType.New => AlertState.New,
            FilterType.Resolved => AlertState.Resolved,
            _ => null
        };

        var specification = alertState is null
            ? new DevicesWithAlertsSpecification(request.SerialNumber)
            : new DevicesWithAlertsSpecification(request.SerialNumber, alertState.Value);

        if (!await _repository.ContainsAsync(specification, cancellationToken))
        {
            return Error.NotFound(
                "Device.NotFound",
                $"Device with serial number '{request.SerialNumber}' was not found.");
        }

        var itemsCount = await
            _repository
                .GetQueryable(specification)
                .SelectMany(x => x.Alerts)
                .CountAsync(cancellationToken).ConfigureAwait(false);

        if (itemsCount == 0)
        {
            return new PagedList<LogItem>(
                Enumerable.Empty<LogItem>(),
                0,
                request.Params.PageNumber,
                request.Params.PageSize);
        }

        var skip = request.Params.PageSize * (request.Params.PageNumber - 1);
        var take = request.Params.PageSize;

        specification = alertState is null
            ? new DevicesWithAlertsSpecification(request.SerialNumber, skip, take)
            : new DevicesWithAlertsSpecification(request.SerialNumber, alertState.Value, skip, take);

        var device = await
            _repository.GetQueryable(specification)
                       .SingleAsync(cancellationToken)
                       .ConfigureAwait(false);

        var logItems = ComputeLogItems(device, cancellationToken);

        return new PagedList<LogItem>(logItems, itemsCount, request.Params.PageNumber, request.Params.PageSize);
    }

    private static IEnumerable<LogItem> ComputeLogItems(Device device, CancellationToken cancellationToken)
    {
        return device.Alerts
            .AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount / 2)
            .WithCancellation(cancellationToken)
            .GroupJoin(
                device.DeviceReadings.AsParallel(),
                alert => alert.DeviceSerialNumber,
                reading => reading.DeviceSerialNumber,
                (alert, readings) => new LogItem
                {
                    AlertType = alert.AlertType,
                    Message = alert.Message,
                    AlertState = alert.AlertState,
                    DateTimeCreated = alert.CreatedDateTime,
                    DateTimeReported = alert.ReportedDateTime,
                    DateTimeLastReported = alert.LastReportedDateTime,
                    MinValue = alert.AlertType switch
                    {
                        AlertType.OutOfRangeTemp => readings.AsParallel().Min(x => x.Temperature),
                        AlertType.OutOfRangeCo => readings.AsParallel().Min(x => x.CarbonMonoxide),
                        AlertType.OutOfRangeHumidity => readings.AsParallel().Min(x => x.Humidity),
                        AlertType.DangerousCoLevel => readings.AsParallel().Min(x => x.CarbonMonoxide),
                        _ => 0m
                    },
                    MaxValue = alert.AlertType switch
                    {
                        AlertType.OutOfRangeTemp => readings.AsParallel().Max(x => x.Temperature),
                        AlertType.OutOfRangeCo => readings.AsParallel().Max(x => x.CarbonMonoxide),
                        AlertType.OutOfRangeHumidity => readings.AsParallel().Max(x => x.Humidity),
                        AlertType.DangerousCoLevel => readings.AsParallel().Max(x => x.CarbonMonoxide),
                        _ => 0m
                    }
                });
    }
}