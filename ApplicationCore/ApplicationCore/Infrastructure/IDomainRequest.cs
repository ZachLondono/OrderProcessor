using MediatR;

namespace ApplicationCore.Infrastructure;

public interface IQuery<TResponse> : IBaseDomainRequest, IRequest<Response<TResponse>> { }

public interface ICommand : IBaseDomainRequest, IRequest<Response> { }


// TODO: create an ICommand with a response