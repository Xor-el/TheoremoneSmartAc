using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Specifications.Devices;
using SmartAc.Domain;

namespace SmartAc.Application.Features.Devices.Get;


internal sealed class GetDeviceQueryHandler : IRequestHandler<GetDeviceQuery, ErrorOr<Device>>
{
    private readonly IRepository<Device> _repository;
    private readonly ILogger<GetDeviceQueryHandler> _logger;

    public GetDeviceQueryHandler(IRepository<Device> repository, ILogger<GetDeviceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ErrorOr<Device>> Handle(GetDeviceQuery request, CancellationToken cancellationToken)
    {
        var specification =
            new DevicesWithRegistrationsSpecification(request.SerialNumber, request.SharedSecret);

        if (await _repository.ContainsAsync(specification, cancellationToken))
        {
            return await _repository.GetQueryable(specification).SingleAsync(cancellationToken).ConfigureAwait(false);
        }

        _logger.LogDebug(
            "There is no matching device for serial number {serialNumber} and the secret provided.",
            request.SerialNumber);

        return Error.NotFound(
            "Device.NotFound",
            $"Device with serial number '{request.SerialNumber}' was not found.");
    }
}