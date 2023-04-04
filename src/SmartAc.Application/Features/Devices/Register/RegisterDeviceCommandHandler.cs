using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Abstractions.Services;
using SmartAc.Application.Specifications.Devices;
using SmartAc.Domain;

namespace SmartAc.Application.Features.Devices.Register;

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

    public Task<ErrorOr<string>> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<Device>();

        var device = repository
            .Find(new DevicesWithRegistrationsSpecification(request.SerialNumber, request.SharedSecret))
            .SingleOrDefault();

        if (device is null)
        {
            var error = Error.NotFound(
                "Device.NotFound",
                $"Device with serial number '{request.SerialNumber}' and provided secret was not found.");

            return Task.FromResult<ErrorOr<string>>(error);
        }

        var (tokenId, jwtToken) =
            _jwtService.GenerateJwtFor(request.SerialNumber, _jwtService.JwtScopeDeviceIngestionService);

        var newRegistrationDevice = new DeviceRegistration
        {
            DeviceSerialNumber = device.SerialNumber,
            TokenId = tokenId
        };

        device.AddRegistration(newRegistrationDevice, request.FirmwareVersion);

        repository.Update(device);

        _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("JWT Token created for device with serial number '{SerialNumber}'", request.SerialNumber);

        return Task.FromResult<ErrorOr<string>>(jwtToken);
    }
}