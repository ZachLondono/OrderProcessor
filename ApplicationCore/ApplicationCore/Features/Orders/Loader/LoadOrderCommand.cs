using ApplicationCore.Features.Orders.Loader.Commands;
using ApplicationCore.Features.Orders.Loader.Providers;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Loader;

public class LoadOrderCommand {

    public record Command(OrderSourceType SourceType, string Source, IOrderLoadingViewModel? OrderLoadingViewModel = null) : ICommand<Order>;

    public class Handler : CommandHandler<Command, Order> {

        private readonly IOrderProviderFactory _factory;
        private readonly IBus _bus;

        public Handler(IOrderProviderFactory factory, IBus bus) {
            _factory = factory;
            _bus = bus;
        }

        public override async Task<Response<Order>> Handle(Command request) {

            var provider = _factory.GetOrderProvider(request.SourceType);
            provider.OrderLoadingViewModel = request.OrderLoadingViewModel;

            OrderData? data = await provider.LoadOrderData(request.Source);

            if (data is null) {

                return new(new Error() {
                    Title = "No order was read",
                    Details = "No data could read from the provided order source."
                });

            }

            var billing = new BillingInfo() {
                InvoiceEmail = null,
                PhoneNumber = "",
                Address = new()
            };

            Response<Order> result = await _bus.Send(new CreateNewOrder.Command(request.Source, data.Number, data.Name, data.Customer, data.VendorId, data.Comment, data.OrderDate, data.Shipping, billing, data.Tax, data.PriceAdjustment, data.Rush, data.Info, data.Products, data.AdditionalItems));

            return result;

        }

    }

}
