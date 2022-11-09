using MediatR;

namespace ApplicationCore.Infrastructure;

// A query represents a request for data from a source, it's result can be cached
public interface IQuery<TResponse> : IBaseDomainRequest, IRequest<Response<TResponse>> { }