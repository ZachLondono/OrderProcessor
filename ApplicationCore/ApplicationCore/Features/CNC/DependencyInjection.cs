using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Features.CNC.ReleasePDF.Configuration;
using ApplicationCore.Features.CNC.ReleasePDF.Dialog;
using ApplicationCore.Features.CNC.ReleasePDF.Services;
using ApplicationCore.Features.CNC.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.CNC;

public static class DependencyInjection {

    public static IServiceCollection AddCNC(this IServiceCollection services, IConfiguration configuration) {
        services.AddTransient<ReleasePDFDecoratorFactory>();
        services.AddTransient<ICNCReleaseDecorator, CNCReleaseDecorator>();
        services.Configure<PDFConfiguration>(configuration.GetRequiredSection("ReleasePDFConfig"));
        services.AddTransient<ReleasePDFDialogViewModel>();
        services.AddToolEditor();
        return services;

    }

}
