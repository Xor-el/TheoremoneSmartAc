using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Abstractions.Services;
using SmartAc.Application.Specifications.Devices;
using SmartAc.Domain;

namespace SmartAc.Application.Features.Devices.Registration;

internal sealed class RegisterDeviceCommandHandler : IRequestHandler<RegisterDeviceCommand, ErrorOr<string>>
{
    private readonly IRepository<Device> _repository;
    private readonly ILogger<RegisterDeviceCommandHandler> _logger;
    private readonly ISmartAcJwtService _jwtService;

    public RegisterDeviceCommandHandler(
        IRepository<Device> repository,
        ILogger<RegisterDeviceCommandHandler> logger,
        ISmartAcJwtService jwtService)
    {
        _logger = logger;
        _repository = repository;
        _jwtService = jwtService;
    }

    public async Task<ErrorOr<string>> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        var specification = new DevicesWithRegistrationsSpecification(request.SerialNumber, request.SharedSecret, true);

        if (!await _repository.ContainsAsync(specification, cancellationToken))
        {
            return Error.NotFound(
                "Device.NotFound",
                $"Device with serial number '{request.SerialNumber}' and provided secret was not found.");
        }

        var device =
            await _repository.GetQueryable(specification).SingleAsync(cancellationToken).ConfigureAwait(false);

        var (tokenId, jwtToken) =
            _jwtService.GenerateJwtFor(request.SerialNumber, _jwtService.JwtScopeDeviceIngestionService);

        var registration = new DeviceRegistration
        {
            DeviceSerialNumber = device.SerialNumber,
            TokenId = tokenId
        };

        device.AddRegistration(registration, request.FirmwareVersion);

        _repository.Update(device);

        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("JWT Token created for device with serial number '{SerialNumber}'", request.SerialNumber);

        return jwtToken;
    }
}