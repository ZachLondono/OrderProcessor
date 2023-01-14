using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;
using System.Data;
using System.Diagnostics;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public class CreateNewOrder {

    public record Command(string Source, string Number, string Name, Guid CustomerId, Guid VendorId, string Comment, DateTime OrderDate, decimal Tax, decimal Shipping, decimal PriceAdjustment, bool Rush, IReadOnlyDictionary<string, string> Info, IEnumerable<IProduct> Products, IEnumerable<AdditionalItem> AdditionalItems, Guid? OrderId = null) : ICommand<Order>;

    public class Handler : CommandHandler<Command, Order> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Order>> Handle(Command request) {

            Order order = Order.Create(request.Source, request.Number, request.Name, request.CustomerId, request.VendorId, request.Comment, request.OrderDate, request.Tax, request.Shipping, request.PriceAdjustment, request.Rush, request.Info, request.Products, request.AdditionalItems, request.OrderId);

            using var connection = _factory.CreateConnection();

            connection.Open();
            var trx = connection.BeginTransaction();

            try {

                const string command = @"INSERT INTO orders (id, source, status, number, name, customerid, vendorid, productionnote, customercomment, orderdate, releasedate, productiondate, completedate, info, tax, shipping, priceadjustment, rush)
                                        VALUES (@Id, @Source, @Status, @Number, @Name, @CustomerId, @VendorId, @ProductionNote, @CustomerComment, @OrderDate, @ReleaseDate, @ProductionDate, @CompleteDate, @Info, @Tax, @Shipping, @PriceAdjustment, @Rush);";

                await connection.ExecuteAsync(command, new {
                    order.Id,
                    order.Source,
                    Status = order.Status.ToString(),
                    order.Name,
                    order.Number,
                    order.CustomerId,
                    order.VendorId,
                    order.ProductionNote,
                    order.CustomerComment,
                    order.OrderDate,
                    order.ReleaseDate,
                    order.ProductionDate,
                    order.CompleteDate,
                    Info = (IDictionary<string, string>)order.Info,
                    order.Tax,
                    order.Shipping,
                    order.PriceAdjustment,
                    order.Rush
                }, trx);

                foreach (var item in request.AdditionalItems) {

                    await CreateItem(item, order.Id, connection, trx);

                }

                trx.Commit();

            } catch (Exception ex) {
                trx.Rollback();
                Debug.WriteLine(ex);
            } finally {
                connection.Close();
            }

            return new(order);

        }

        private static async Task CreateItem(AdditionalItem item, Guid orderId, IDbConnection connection, IDbTransaction transaction) {

            const string itemCommand = @"INSERT INTO additionalitems (id, orderid, description, price)
                                        VALUES (@Id, @OrderId, @Description, @Price)";

            await connection.ExecuteAsync(itemCommand, new {
                item.Id,
                OrderId = orderId,
                item.Description,
                item.Price
            }, transaction);

        }

    }

}
