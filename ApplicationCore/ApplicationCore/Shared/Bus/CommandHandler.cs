using MediatR;

namespace ApplicationCore.Infrastructure.Bus;

public abstract class CommandHandler<TCommand> : IRequestHandler<TCommand, Response> where TCommand : ICommand {

    public async Task<Response> Handle(TCommand request, CancellationToken cancellationToken) => await Handle(request);

    // TODO: add cancellation token
    public abstract Task<Response> Handle(TCommand command);

}


public abstract class CommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Response<TResponse>> where TCommand : ICommand<TResponse> {

    public async Task<Response<TResponse>> Handle(TCommand request, CancellationToken cancellationToken) => await Handle(request);

    // TODO: add cancellation token
    public abstract Task<Response<TResponse>> Handle(TCommand command);

}