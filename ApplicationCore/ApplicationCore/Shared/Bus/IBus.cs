namespace ApplicationCore.Infrastructure.Bus;

public interface IBus {
    Task<Response<TResponse>> Send<TResponse>(IQuery<TResponse> request, CancellationToken cancellationToken = default);
    Task<Response> Send(ICommand request, CancellationToken cancellationToken = default);
    Task<Response<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}
