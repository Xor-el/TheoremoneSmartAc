using MediatR;
using SmartAc.Application.Contracts;

namespace SmartAc.Application.Features.DeviceReadings.StoreReadings;

public sealed record StoreReadingsCommand(string SerialNumber, IEnumerable<SensorReading> Readings) : IRequest;