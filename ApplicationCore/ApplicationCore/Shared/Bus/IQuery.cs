using MediatR;

namespace ApplicationCore.Infrastructure.Bus;

// A query represents a request for data from a source, it's result can be cached
public interface IQuery<TResponse> : IBaseDomainRequest, IRequest<Response<TResponse>> { }