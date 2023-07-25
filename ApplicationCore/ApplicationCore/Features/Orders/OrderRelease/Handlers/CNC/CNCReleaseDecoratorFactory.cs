using ApplicationCore.Features.CNC.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;

public class CNCReleaseDecoratorFactory {

    private readonly IServiceProvider _serviceProvider;

    public CNCReleaseDecoratorFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public async Task<(ICNCReleaseDecorator, ReleasedJob?)> Create(string reportFilePath, DateTime orderDate, string customerName, string vendorName) {

        var decorator = _serviceProvider.GetRequiredService<ICNCReleaseDecorator>();
        var job = await decorator.LoadDataFromFile(reportFilePath, orderDate, customerName, vendorName);
        return (decorator, job);

    }

}