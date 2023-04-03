using MediatR;
using Microsoft.Extensions.Options;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Extensions;
using SmartAc.Application.Options;
using SmartAc.Application.Specifications.Alerts;
using SmartAc.Domain;

namespace SmartAc.Application.Features.Devices.StoreReadings;

internal sealed class StoreReadingsCommandHandler : IRequestHandler<StoreReadingsCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SensorParams _sensorParams;

    public StoreReadingsCommandHandler(IUnitOfWork unitOfWork, IOptions<SensorParams> parameters)
    {
        _unitOfWork = unitOfWork;
        _sensorParams = parameters.Value;
    }

    public async Task Handle(StoreReadingsCommand request, CancellationToken cancellationToken)
    {
        var readings =
            request.Readings
                .Select(reading => reading.ToDeviceReading(request.SerialNumber))
                .OrderBy(x => x.RecordedDateTime);

        foreach (var reading in readings)
        {
            var saveTask = SaveReadingToDb(reading, cancellationToken);
            var alertsTask = ProcessPotentialAlerts(reading, cancellationToken);

            await Task.WhenAll(saveTask, alertsTask);
            await TryResolveErrorStates(reading, cancellationToken);
        }
    }

    private async Task SaveReadingToDb(DeviceReading reading, CancellationToken cancellationToken = default)
    {
        _unitOfWork.GetRepository<DeviceReading>().Add(reading);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessPotentialAlerts(DeviceReading reading, CancellationToken cancellationToken = default)
    {
        IRepository<Alert> alertRepository = _unitOfWork.GetRepository<Alert>();

        var alerts =
            reading.GetPotentialAlerts(_sensorParams)
                   .OrderBy(x => x.DateTimeReported);

        foreach (var alert in alerts)
        {
            var specification = new AlertsSpecification(reading.DeviceSerialNumber, alert.AlertType);

            if (!await alertRepository.ContainsAsync(specification, cancellationToken))
            {
                alertRepository.Add(alert);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                continue;
            }

            var dbAlert = alertRepository.Find(specification).First();

            var diff = (alert.DateTimeReported - dbAlert.DateTimeReported).TotalMinutes;

            var alertState = diff <= _sensorParams.ReadingTimespanMinutes
                ? dbAlert.AlertState == AlertState.Resolved ? AlertState.New : dbAlert.AlertState
                : AlertState.Resolved;

            dbAlert.Update(alert.DateTimeReported, alert.Message, alertState);

            alertRepository.Update(dbAlert);

            if (diff > _sensorParams.ReadingTimespanMinutes)
            {
                alertRepository.Add(alert);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task TryResolveErrorStates(DeviceReading reading, CancellationToken cancellationToken)
    {
        IRepository<Alert> alertRepository = _unitOfWork.GetRepository<Alert>();

        var specification = new AlertsSpecification(reading.DeviceSerialNumber, AlertState.New);

        var alerts = alertRepository.Find(specification).ToList();

        foreach (var alert in alerts.Where(alert => Helpers.Helpers.IsResolved(alert.AlertType, reading, _sensorParams)))
        {
            alert.Update(reading.RecordedDateTime, alert.Message, AlertState.Resolved);
            alertRepository.Update(alert);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}