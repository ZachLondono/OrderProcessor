using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Commands;

public class OverwriteExistingOrderWithId {

    public record Command(Guid ExistingId, string Source, string Number, string Name, Guid CustomerId, Guid VendorId, string Comment, DateTime OrderDate, decimal Tax, decimal Shipping, decimal PriceAdjustment, IReadOnlyDictionary<string, string> Info, IEnumerable<DrawerBox> Boxes, IEnumerable<AdditionalItem> AdditionalItems) : ICommand<Order>;

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
            const string deleteAdditionalItemCommand = "DELETE FROM drawerboxes WHERE orderid = @ExistingId;";

            using var connection = _factory.CreateConnection();

            connection.Open();
            var trx = connection.BeginTransaction();

            try {
                await connection.ExecuteAsync(deleteOrderCommand, request, trx);
                await connection.ExecuteAsync(deleteBoxesCommand, request, trx);
                await connection.ExecuteAsync(deleteAdditionalItemCommand, request, trx);
                trx.Commit();
                connection.Close();
            } catch(Exception ex) {
                trx.Rollback();
                connection.Close();
                return new(new Error() { Message = ex.ToString() });
            }

            return await _bus.Send(new CreateNewOrder.Command(request.Source, request.Number, request.Name, request.CustomerId, request.VendorId, request.Comment, request.OrderDate, request.Tax, request.Shipping, request.PriceAdjustment, request.Info, request.Boxes, request.AdditionalItems, request.ExistingId));

        }
    }

}
