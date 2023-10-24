using MediatR;

namespace ApplicationCore.Infrastructure.Bus;

public interface ICommand : IRequest<Response> { }
public interface ICommand<TResponse> : IRequest<Response<TResponse>> { }