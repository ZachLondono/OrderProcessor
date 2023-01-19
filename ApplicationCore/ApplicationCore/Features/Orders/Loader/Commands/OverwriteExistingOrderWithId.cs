using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public class OverwriteExistingOrderWithId {

    public record Command(Guid ExistingId, string Source, string Number, string Name, Customer Customer, Guid VendorId, string Comment, DateTime OrderDate, ShippingInfo Shipping, BillingInfo Billing, decimal Tax, decimal PriceAdjustment, bool Rush, IReadOnlyDictionary<string, string> Info, IEnumerable<IProduct> Products, IEnumerable<AdditionalItem> AdditionalItems) : ICommand<Order>;

    public class Handler : CommandHandler<Command, Order> {

        private readonly IBus _bus;
        private readonly IDbConnectionFactory _factory;

        public Handler(IBus bus, IDbConnectionFactory factory) {
            _bus = bus;
            _factory = factory;
        }

        public override async Task<Response<Order>> Handle(Command request) {

            const string deleteOrderCommand = "DELETE FROM orders WHERE id = @ExistingId;";
            const string deleteBoxesCommand = "DELETE FROM additionalitems WHERE orderid = @ExistingId;";

            using var connection = _factory.CreateConnection();

            connection.Open();
            var trx = connection.BeginTransaction();

            try {
                await connection.ExecuteAsync(deleteOrderCommand, request, trx);
                await connection.ExecuteAsync(deleteBoxesCommand, request, trx);
                trx.Commit();
                connection.Close();
            } catch (Exception ex) {
                trx.Rollback();
                connection.Close();
                return new(new Error() {
                    Title = "Exception thrown while removing exisitng order data",
                    Details = ex.ToString()
                });
            }

            return await _bus.Send(new CreateNewOrder.Command(request.Source, request.Number, request.Name, request.Customer, request.VendorId, request.Comment, request.OrderDate, request.Shipping, request.Billing, request.Tax, request.PriceAdjustment, request.Rush, request.Info, request.Products, request.AdditionalItems, request.ExistingId));

        }
    }

}
