using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Extensions;
using SmartAc.Application.Helpers;
using SmartAc.Application.Options;
using SmartAc.Application.Specifications.Alerts;
using SmartAc.Domain;

namespace SmartAc.Application.Events;

internal sealed class ReadingsStoredEventHandler : INotificationHandler<ReadingsStoredEvent>
{
    private readonly IRepository<Alert> _repository;
    private readonly SensorParams _sensorParams;

    public ReadingsStoredEventHandler(IRepository<Alert> repository, IOptions<SensorParams> parameters)
    {
        _repository = repository;
        _sensorParams = parameters.Value;
    }

    public async Task Handle(ReadingsStoredEvent notification, CancellationToken cancellationToken)
    {
        foreach (var reading in notification.Readings.OrderBy(r => r.RecordedDateTime))
        {
            var alertIds = await ProcessPotentialAlerts(reading, cancellationToken).ConfigureAwait(false);
            await TryResolveErrorStates(alertIds, reading, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<int[]> ProcessPotentialAlerts(DeviceReading reading,
        CancellationToken cancellationToken = default)
    {
        var alerts =
            reading.GetPotentialAlerts(_sensorParams)
                   .OrderBy(x => x.ReportedDateTime)
                   .ToList();

        foreach (var alert in alerts)
        {
            var specification = new AlertsSpecification(reading.DeviceSerialNumber, alert.AlertType);

            if (!await _repository.ContainsAsync(specification, cancellationToken))
            {
                _repository.Add(alert);
                await _repository.SaveChangesAsync(cancellationToken);
                continue;
            }

            var alertFromDb = await
                _repository
                    .GetQueryable(specification)
                    .FirstAsync(cancellationToken);

            var diff = (alert.ReportedDateTime - alertFromDb.ReportedDateTime).TotalMinutes;

            var alertState = diff <= _sensorParams.ReadingTimespanMinutes
                ? alertFromDb.AlertState == AlertState.Resolved ? AlertState.New : alertFromDb.AlertState
                : AlertState.Resolved;

            alertFromDb.Update(alert.ReportedDateTime, alert.Message, alertState);

            _repository.Update(alertFromDb);

            if (alertState == AlertState.Resolved)
            {
                _repository.Add(alert);
            }

            await _repository.SaveChangesAsync(cancellationToken);
        }

        return alerts.Select(x => x.AlertId).ToArray();
    }

    private async Task TryResolveErrorStates(int[] alertIdsToExclude, DeviceReading reading,
        CancellationToken cancellationToken)
    {
        var specification =
            new AlertsMatchingStateExcludingIdsSpecification(reading.DeviceSerialNumber, AlertState.New,
                alertIdsToExclude);

        var alerts = await
            _repository.GetQueryable(specification).ToListAsync(cancellationToken);

        foreach (var alert in alerts.Where(alert => AlertHelpers.IsResolved(alert.AlertType, reading, _sensorParams)))
        {
            alert.Update(reading.RecordedDateTime, alert.Message, AlertState.Resolved);
            _repository.Update(alert);
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }
}