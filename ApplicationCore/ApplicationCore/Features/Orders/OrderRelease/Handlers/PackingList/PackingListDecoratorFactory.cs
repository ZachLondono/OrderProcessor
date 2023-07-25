using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.PackingList;

public class PackingListDecoratorFactory {

    private readonly IServiceProvider _serviceProvider;

    public PackingListDecoratorFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public async Task<IPackingListDecorator> CreateDecorator(Order order) {

        var decorator = _serviceProvider.GetRequiredService<IPackingListDecorator>();
        await decorator.AddData(order);
        return decorator;

    }

}
