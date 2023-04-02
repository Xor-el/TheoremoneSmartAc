using MediatR;
using SmartAc.Application.Contracts;

namespace SmartAc.Application.Features.Devices.SaveReadings;

public sealed record SubmitReadingsCommand(string SerialNumber, IEnumerable<SensorReading> Readings) : IRequest;