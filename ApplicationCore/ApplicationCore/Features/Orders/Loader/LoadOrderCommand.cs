using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Providers;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Loader;

public class LoadOrderCommand {

    public record Command(OrderSource Source) : IQuery<Order>;

    public class Handler : QueryHandler<Command, Order> {

        private IOrderProviderFactory _factory;

        public Handler(IOrderProviderFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Order>> Handle(Command request) {

            var provider = _factory.GetOrderProvider(request.Source);

            var data = await provider.LoadOrderData();

            return new(data);

        }

    }

}
