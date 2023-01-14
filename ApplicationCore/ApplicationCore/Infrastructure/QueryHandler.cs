using MediatR;

namespace ApplicationCore.Infrastructure;

public abstract class QueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Response<TResponse>> where TQuery : IQuery<TResponse> {

    public async Task<Response<TResponse>> Handle(TQuery request, CancellationToken cancellationToken) => await Handle(request);

    // TODO: add cancellation token
    public abstract Task<Response<TResponse>> Handle(TQuery query);

}