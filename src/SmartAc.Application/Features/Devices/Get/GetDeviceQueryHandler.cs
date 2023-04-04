using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Specifications.Devices;
using SmartAc.Domain;

namespace SmartAc.Application.Features.Devices.Get;


internal sealed class GetDeviceQueryHandler : IRequestHandler<GetDeviceQuery, ErrorOr<Device>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDeviceQueryHandler> _logger;

    public GetDeviceQueryHandler(IUnitOfWork unitOfWork, ILogger<GetDeviceQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<Device>> Handle(GetDeviceQuery request, CancellationToken cancellationToken)
    {
        IRepository<Device> repo = _unitOfWork.GetRepository<Device>();

        var specification =
            new DevicesWithRegistrationsSpecification(request.SerialNumber, request.SharedSecret);

        if (await repo.ContainsAsync(specification, cancellationToken))
        {
            return repo.Find(specification).Single();
        }

        _logger.LogDebug(
            "There is no matching device for serial number {serialNumber} and the secret provided.",
            request.SerialNumber);

        return Error.NotFound(
            "Device.NotFound",
            $"Device with serial number '{request.SerialNumber}' was not found.");
    }
}