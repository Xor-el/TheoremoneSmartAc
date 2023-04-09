using MediatR;
using SmartAc.Domain;

namespace SmartAc.Application.Events;

internal sealed record ReadingsStoredEvent(List<DeviceReading> Readings) : INotification;