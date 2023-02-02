using ApplicationCore.Features.Shared;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.WorkOrders;

public static class DependencyInjection {

    public static IServiceCollection AddWorkOrders(this IServiceCollection services) {

        services.AddTransient<Manufacturing.CreateWorkOrder>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (orderId, name, productIds) => {
                var command = new CreateWorkOrder.Command(name, orderId, productIds);
                var response = await bus.Send(command);
                response.OnError(error => throw new InvalidOperationException($"{error.Title} - {error.Details}"));
            };

        });

        services.AddTransient<Manufacturing.IsProductComplete>(sp => {

            var bus = sp.GetRequiredService<IBus>();
            return async (orderId, productId) => {
                
                var command = new IsProductComplete.Query(orderId, productId);
                var response = await bus.Send(command);
                
                bool isComplete = false;
                response.Match(
                    result => isComplete = result,
                    error => throw new InvalidOperationException($"{error.Title} - {error.Details}")
                );

                return isComplete;

            };

        });

        return services;

    }

}
