using MediatR;
using SmartAc.Domain;

namespace SmartAc.Application.Features.DeviceReadings.StoreReadings;

public sealed record StoreReadingsCommand(IEnumerable<DeviceReading> Readings) : IRequest;