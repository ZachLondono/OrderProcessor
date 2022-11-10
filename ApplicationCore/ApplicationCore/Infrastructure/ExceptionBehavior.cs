using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Infrastructure;

internal class ExceptionBehaviorA<TRequest, TSuccess> : IPipelineBehavior<TRequest, Response<TSuccess>> where TRequest : IRequest<Response<TSuccess>> {

    public Task<Response<TSuccess>> Handle(TRequest request, RequestHandlerDelegate<Response<TSuccess>> next, CancellationToken cancellationToken) {
        
        try {

            return next();

        } catch (Exception e) {

            var error = new Error() {
                Title = "Error fulfilling request",
                Details = e.ToString(),
            };

            var response = new Response<TSuccess>(error);

            return Task.FromResult(response);

        }

    }

}

internal class ExceptionBehaviorB<TRequest> : IPipelineBehavior<TRequest, Response> where TRequest : IRequest<Response> {

    public Task<Response> Handle(TRequest request, RequestHandlerDelegate<Response> next, CancellationToken cancellationToken) {

        try {

            return next();

        } catch (Exception e) {

            var error = new Error() {
                Title = "Error fulfilling request",
                Details = e.ToString(),
            };

            var response = new Response(error);

            return Task.FromResult(response);

        }

    }

}