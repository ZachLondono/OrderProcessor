using MediatR;

namespace ApplicationCore.Infrastructure;

public abstract class CommandHandler<TCommand> : IRequestHandler<TCommand, Response> where TCommand : ICommand {

    public async Task<Response> Handle(TCommand request, CancellationToken cancellationToken) => await Handle(request);

    // TODO: add cancellation token
    public abstract Task<Response> Handle(TCommand command);

}
