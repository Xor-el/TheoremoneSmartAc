using ErrorOr;
using MediatR;

namespace SmartAc.Application.Features.Devices.Registration;

public sealed record RegisterDeviceCommand
    (string SerialNumber, string SharedSecret, string FirmwareVersion) : IRequest<ErrorOr<string>>;
