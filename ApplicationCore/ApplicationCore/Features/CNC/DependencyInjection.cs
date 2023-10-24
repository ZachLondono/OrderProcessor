using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.CNC.ReleaseDialog;
using ApplicationCore.Features.CNC.ReleaseEmail;
using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Features.CNC.ReleasePDF.Services;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.CNC;

public static class DependencyInjection {

    public static IServiceCollection AddCNC(this IServiceCollection services, IConfiguration configuration) {
        services.AddTransient<ReleasePDFDecoratorFactory>();
        services.AddTransient<ICNCReleaseDecorator, CNCReleaseDecorator>();
        services.Configure<PDFConfiguration>(configuration.GetRequiredSection("ReleasePDFConfig"));
        services.AddTransient<ReleasePDFDialogViewModel>();
        services.AddTransient<ReleaseEmailBodyGenerator>();
        return services;

    }

}
