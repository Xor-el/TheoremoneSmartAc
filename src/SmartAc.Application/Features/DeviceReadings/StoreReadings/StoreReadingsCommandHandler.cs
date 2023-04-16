using MediatR;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Events;
using SmartAc.Domain;

namespace SmartAc.Application.Features.DeviceReadings.StoreReadings;

internal sealed class StoreReadingsCommandHandler : IRequestHandler<StoreReadingsCommand>
{
    private readonly IPublisher _publisher;
    private readonly IRepository<DeviceReading> _repository;

    public StoreReadingsCommandHandler(IRepository<DeviceReading> repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public Task Handle(StoreReadingsCommand request, CancellationToken cancellationToken)
    {
        _repository.AddRange(request.Readings);
        _repository.SaveChangesAsync(cancellationToken);
        _publisher.Publish(new ReadingsStoredEvent(request.Readings), cancellationToken);
        return Task.CompletedTask;
    }
}