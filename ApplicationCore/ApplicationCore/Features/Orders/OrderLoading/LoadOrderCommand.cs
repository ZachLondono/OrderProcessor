using Domain.Infrastructure.Bus;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using OrderLoading;

namespace ApplicationCore.Features.Orders.OrderLoading;

public class LoadOrderCommand {

    public record Command(OrderSourceType SourceType, string Source, IOrderLoadWidgetViewModel? OrderLoadingViewModel = null) : ICommand<OrderData?>;

    public class Handler(IOrderProviderFactory factory) : CommandHandler<Command, OrderData?> {

        private readonly IOrderProviderFactory _factory = factory;

        public override async Task<Response<OrderData?>> Handle(Command request) {

            IOrderProvider provider = _factory.GetOrderProvider(request.SourceType);
            provider.OrderLoadingViewModel = request.OrderLoadingViewModel;

            OrderData? data = await provider.LoadOrderData(request.Source);

            if (data is null) {

                return new(new Error() {
                    Title = "No order was read",
                    Details = "No data could read from the provided order source."
                });

            }

            return data;

        }

    }

}
