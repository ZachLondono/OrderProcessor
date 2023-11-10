using ApplicationCore.Shared.CNC.ReleasedJob;
using ApplicationCore.Shared.CNC.ReleasePDF;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;

public class CNCReleaseDecoratorFactory {

    private readonly IServiceProvider _serviceProvider;

    public CNCReleaseDecoratorFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public ICNCReleaseDecorator Create(ReleasedJob job) {

        var decorator = _serviceProvider.GetRequiredService<ICNCReleaseDecorator>();
        decorator.AddData(job);
        return decorator;

    }

}