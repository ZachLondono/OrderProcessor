using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Loader.Commands;
using ApplicationCore.Features.Orders.Loader.Providers;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Loader;

public class LoadOrderCommand {

    public record Command(OrderSourceType SourceType, string Source) : ICommand<Order>;

    public class Handler : CommandHandler<Command, Order> {

        private readonly IOrderProviderFactory _factory;
        private readonly IBus _bus;

        public Handler(IOrderProviderFactory factory, IBus bus) {
            _factory = factory;
            _bus = bus;
        }

        public override async Task<Response<Order>> Handle(Command request) {

            var provider = _factory.GetOrderProvider(request.SourceType);

            var data = await provider.LoadOrderData(request.Source);

            if (data is null) {

                return new(new Error() {
                    Title = "No order was read",
                    Details = "No data could read from the provided order source."
                });

            }

            Response<Order> result = await _bus.Send(new CreateNewOrder.Command(request.Source, data.Number, data.Name, data.CustomerId, data.VendorId, data.Comment, data.OrderDate, data.Tax, data.Shipping, data.PriceAdjustment, data.Rush, data.Info, data.Products, data.AdditionalItems));

            return result;

        }

    }

}
