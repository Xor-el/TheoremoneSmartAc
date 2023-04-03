using MediatR;
using SmartAc.Application.Contracts;

namespace SmartAc.Application.Features.Devices.StoreReadings;

public sealed record StoreReadingsCommand(string SerialNumber, IEnumerable<SensorReading> Readings) : IRequest;