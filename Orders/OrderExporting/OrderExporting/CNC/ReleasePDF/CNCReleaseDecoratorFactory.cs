using Microsoft.Extensions.DependencyInjection;
using OrderExporting.CNC.Programs.Job;

namespace OrderExporting.CNC.ReleasePDF;

public class CNCReleaseDecoratorFactory
{

    private readonly IServiceProvider _serviceProvider;

    public CNCReleaseDecoratorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ICNCReleaseDecorator Create(ReleasedJob job)
    {

        var decorator = _serviceProvider.GetRequiredService<ICNCReleaseDecorator>();
        decorator.AddData(job);
        return decorator;

    }

}