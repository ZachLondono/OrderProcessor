using MediatR;

namespace Domain.Infrastructure.Bus;

public interface IQuery<TResponse> : IRequest<Response<TResponse>> { }