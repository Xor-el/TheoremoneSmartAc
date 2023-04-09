using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Extensions;
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

    public Task Handle(ReadingsStoredEvent notification, CancellationToken cancellationToken)
    {
        foreach (var reading in notification.Readings.OrderBy(r => r.RecordedDateTime))
        {
            ProcessPotentialAlerts(reading, cancellationToken).ConfigureAwait(false);
            TryResolveErrorStates(reading, cancellationToken).ConfigureAwait(false);
        }
        return Task.CompletedTask;
    }

    private async Task ProcessPotentialAlerts(DeviceReading reading, CancellationToken cancellationToken = default)
    {
        var alerts =
            reading.GetPotentialAlerts(_sensorParams).OrderBy(x => x.ReportedDateTime);

        foreach (var alert in alerts)
        {
            var specification = new AlertsSpecification(reading.DeviceSerialNumber, alert.AlertType);

            if (!await _repository.ContainsAsync(specification, cancellationToken))
            {
                _repository.Add(alert);
                await _repository.SaveChangesAsync(cancellationToken);
                continue;
            }

            var dbAlert = await
                _repository
                    .GetQueryable(specification)
                    .FirstAsync(cancellationToken);

            var diff = (alert.ReportedDateTime - dbAlert.ReportedDateTime).TotalMinutes;

            var alertState = diff <= _sensorParams.ReadingTimespanMinutes
                ? dbAlert.AlertState == AlertState.Resolved ? AlertState.New : dbAlert.AlertState
                : AlertState.Resolved;

            dbAlert.Update(alert.ReportedDateTime, alert.Message, alertState);

            _repository.Update(dbAlert);

            if (diff > _sensorParams.ReadingTimespanMinutes)
            {
                _repository.Add(alert);
            }

            await _repository.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task TryResolveErrorStates(DeviceReading reading, CancellationToken cancellationToken)
    {
        var specification = new AlertsSpecification(reading.DeviceSerialNumber, AlertState.New);

        var alerts = await
            _repository.GetQueryable(specification).ToListAsync(cancellationToken);

        foreach (var alert in alerts.Where(alert => Helpers.Helpers.IsResolved(alert.AlertType, reading, _sensorParams)))
        {
            alert.Update(reading.RecordedDateTime, alert.Message, AlertState.Resolved);
            _repository.Update(alert);
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }
}