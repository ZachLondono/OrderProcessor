using MediatR;

namespace ApplicationCore.Infrastructure.Bus;

public interface IQuery<TResponse> : IRequest<Response<TResponse>> { }