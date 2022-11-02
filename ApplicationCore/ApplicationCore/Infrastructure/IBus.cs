namespace ApplicationCore.Infrastructure;

public interface IBus {
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : IDomainNotification;
    Task<Response<TResponse>> Send<TResponse>(IQuery<TResponse> request, CancellationToken cancellationToken = default);
    Task<Response> Send(ICommand request, CancellationToken cancellationToken = default);
}
