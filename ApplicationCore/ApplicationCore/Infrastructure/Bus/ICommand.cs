using MediatR;

namespace ApplicationCore.Infrastructure.Bus;

// Commands represent an action to be executed, it's result should not be cached
public interface ICommand : IBaseDomainRequest, IRequest<Response> { }
public interface ICommand<TResponse> : IBaseDomainRequest, IRequest<Response<TResponse>> { }