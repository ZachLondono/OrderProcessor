using Domain.Infrastructure.Bus;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Products;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.AddProductToOrder.AddClosetPartToOrder;

public class InsertClosetPartInOrder {

    public record Command(Guid OrderId, NewClosetPart ClosetPart) : ICommand<ClosetPart>;

    public class Handler : CommandHandler<Command, ClosetPart> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<ClosetPart>> Handle(Command command) {

            using var connection = await _factory.CreateConnection();
            connection.Open();
            var trx = connection.BeginTransaction();

            try {

                var part = CreateProduct(command.OrderId, command.ClosetPart);

                ProductsPersistance.InsertProduct(part, command.OrderId, connection, trx);

                trx.Commit();

                return part;

            } catch (Exception ex) {

                trx.Rollback();

                return new Error() {
                    Title = "Failed to Add Closet Part to Order",
                    Details = ex.Message
                };

            } finally {

                connection.Close();

            }

        }

        private static ClosetPart CreateProduct(Guid orderId, NewClosetPart data) {

            var id = Guid.NewGuid();

            var width = Dimension.FromMillimeters(data.Width);
            var length = Dimension.FromMillimeters(data.Length);

            var material = new ClosetMaterial(data.MaterialFinish, data.MaterialCore);
            ClosetPaint? paint = data.Paint.Match<ClosetPaint?>(
                p => p,
                n => null);

            Dictionary<string, string> parameters = [];
            foreach (var param in data.Parameters) {
                parameters[param.Name] = param.Value;
            }

            var box = new ClosetPart(
                id, data.Qty, data.UnitPrice, data.ProductNumber, data.Room,
                data.Sku, width, length, material, paint, data.EdgeBandingColor, data.Comment, data.InstallCams, parameters);

            return box;

        }

    }

}
