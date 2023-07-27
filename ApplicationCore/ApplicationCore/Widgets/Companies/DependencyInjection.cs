using ApplicationCore.Widgets.Companies.AllmoxyId;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Widgets.Companies;

internal static class DependencyInjection {

    public static IServiceCollection AddCompanyWidgets(this IServiceCollection services) {
        return services.AddTransient<CustomerAllmoxyIdWidgetViewModel>();
    }

}
