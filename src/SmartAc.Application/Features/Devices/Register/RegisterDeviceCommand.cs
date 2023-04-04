using ErrorOr;
using MediatR;

namespace SmartAc.Application.Features.Devices.Register;

public sealed record RegisterDeviceCommand
    (string SerialNumber, string SharedSecret, string FirmwareVersion) : IRequest<ErrorOr<string>>;
