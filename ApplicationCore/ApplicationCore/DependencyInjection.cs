using ApplicationCore.Features.Orders.ProductDrawings.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore; 

public static class DependencyInjection {

    public static IServiceCollection AddViewModels(this IServiceCollection services) {
        return services
                    .AddTransient<ProductDrawingManagerViewModel>()
                    .AddTransient<ProductDrawingManagerButtonViewModel>()
                    .AddTransient<ProductDrawingRowViewModel>();

    }

}