using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.PackingList;

public class PackingListDecoratorFactory {

    private readonly IServiceProvider _serviceProvider;

    public PackingListDecoratorFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public async Task<IPackingListDecorator> CreateDecorator(Order order, bool checkBoxes, bool signatureField) {

        var decorator = _serviceProvider.GetRequiredService<IPackingListDecorator>();
        await decorator.AddData(order);
        decorator.IncludeCheckBoxesNextToItems = checkBoxes;
        decorator.IncludeSignatureField = signatureField;
        return decorator;

    }

}
