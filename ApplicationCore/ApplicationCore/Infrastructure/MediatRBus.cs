using MediatR;

namespace ApplicationCore.Infrastructure;

internal class MediatRBus : IBus {

    private readonly IMediator _mediator;

    public MediatRBus(IMediator mediator) {
        _mediator = mediator;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : IDomainNotification {
        return _mediator.Publish(notification, cancellationToken);
    }

    public Task<Response<TResponse>> Send<TResponse>(IQuery<TResponse> request, CancellationToken cancellationToken = default) {
        return _mediator.Send(request, cancellationToken);
    }

    public Task<Response> Send(ICommand request, CancellationToken cancellationToken = default) {
        return _mediator.Send(request, cancellationToken);
    }

    public Task<Response<TResquest>> Send<TResquest>(ICommand<TResquest> request, CancellationToken cancellationToken = default) {
        return _mediator.Send(request, cancellationToken);
    }
}
