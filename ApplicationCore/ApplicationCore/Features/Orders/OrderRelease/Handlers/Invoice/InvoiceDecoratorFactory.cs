using Domain.Orders.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.Invoice;

public class InvoiceDecoratorFactory {

    private readonly IServiceProvider _serviceProvider;

    public InvoiceDecoratorFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public async Task<IInvoiceDecorator> CreateDecorator(Order order) {

        var decorator = _serviceProvider.GetRequiredService<IInvoiceDecorator>();
        await decorator.AddData(order);
        return decorator;

    }

}
