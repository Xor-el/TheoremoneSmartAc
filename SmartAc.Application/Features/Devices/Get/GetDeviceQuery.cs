using ErrorOr;
using MediatR;
using SmartAc.Domain;

namespace SmartAc.Application.Features.Devices.Get;

public sealed record GetDeviceQuery(
    string SerialNumber,
    string SharedSecret) : IRequest<ErrorOr<Device>>;