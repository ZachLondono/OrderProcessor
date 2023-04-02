using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;

public class CNCReleaseDecoratorFactory {

    private readonly IServiceProvider _serviceProvider;

    public CNCReleaseDecoratorFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public ICNCReleaseDecorator Create(string reportFilePath) {

        var decorator = _serviceProvider.GetRequiredService<ICNCReleaseDecorator>();
        decorator.ReportFilePath = reportFilePath;
        return decorator;

    }

}