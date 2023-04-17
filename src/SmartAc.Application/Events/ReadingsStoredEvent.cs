using MediatR;
using SmartAc.Domain;

namespace SmartAc.Application.Events;

internal sealed record ReadingsStoredEvent(IEnumerable<DeviceReading> Readings) : INotification;