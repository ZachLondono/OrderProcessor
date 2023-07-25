using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

public class JobSummaryDecoratorFactory {

    private readonly IServiceProvider _serviceProvider;

    public JobSummaryDecoratorFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public async Task<IJobSummaryDecorator> CreateDecorator(Order order, bool showItems, bool showSupplies) {
        var decorator = _serviceProvider.GetRequiredService<IJobSummaryDecorator>();
        await decorator.AddData(order, showItems, showSupplies);
        return decorator;
    }

}
