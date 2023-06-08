using ApplicationCore.Features.Orders.Loader.Commands;
using ApplicationCore.Features.Orders.Loader.Providers;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Dialog;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.Loader;

public class LoadOrderCommand {

    public record Command(OrderSourceType SourceType, string Source, IOrderLoadWidgetViewModel? OrderLoadingViewModel = null) : ICommand<OrderData?>;

    public class Handler : CommandHandler<Command, OrderData?> {

        private readonly IOrderProviderFactory _factory;

        public Handler(IOrderProviderFactory factory) {
            _factory = factory;
        }

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

            return Response<OrderData?>.Success(data);

        }

    }

}
