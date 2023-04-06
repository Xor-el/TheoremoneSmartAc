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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterDeviceCommandHandler> _logger;
    private readonly ISmartAcJwtService _jwtService;

    public RegisterDeviceCommandHandler(ILogger<RegisterDeviceCommandHandler> logger, IUnitOfWork unitOfWork, ISmartAcJwtService jwtService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<ErrorOr<string>> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<Device>();

        var specification = new DevicesWithRegistrationsSpecification(request.SerialNumber, request.SharedSecret, true);

        if (!await repository.ContainsAsync(specification, cancellationToken))
        {
            return Error.NotFound(
                "Device.NotFound",
                $"Device with serial number '{request.SerialNumber}' and provided secret was not found.");
        }

        var device = 
            await repository.Find(specification).SingleAsync(cancellationToken).ConfigureAwait(false);

        var (tokenId, jwtToken) =
            _jwtService.GenerateJwtFor(request.SerialNumber, _jwtService.JwtScopeDeviceIngestionService);

        var newRegistrationDevice = new DeviceRegistration
        {
            DeviceSerialNumber = device.SerialNumber,
            TokenId = tokenId
        };

        device.AddRegistration(newRegistrationDevice, request.FirmwareVersion);

        repository.Update(device);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("JWT Token created for device with serial number '{SerialNumber}'", request.SerialNumber);

        return jwtToken;
    }
}