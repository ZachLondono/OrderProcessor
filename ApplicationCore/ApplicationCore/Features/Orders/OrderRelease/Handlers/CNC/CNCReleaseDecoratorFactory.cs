using ApplicationCore.Features.CNC.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;

public class CNCReleaseDecoratorFactory {

    private readonly IServiceProvider _serviceProvider;

    public CNCReleaseDecoratorFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public async Task<(ICNCReleaseDecorator, ReleasedJob?)> Create(string reportFilePath, DateTime orderDate, DateTime? dueDate, string customerName, string vendorName) {

        var decorator = _serviceProvider.GetRequiredService<ICNCReleaseDecorator>();
        var job = await decorator.LoadDataFromFile(reportFilePath, orderDate, dueDate, customerName, vendorName);
        return (decorator, job);

    }

}