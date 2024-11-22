using Domain.Infrastructure.Bus;
using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.Orders.Enums;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Products;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.AddProductToOrder.AddDovetailDBToOrder;

public class InsertDovetailDBProductInOrder {

    public record Command(Guid OrderId, NewDovetailDB DrawerBox) : ICommand<DovetailDrawerBoxProduct>;

    public class Handler : CommandHandler<Command, DovetailDrawerBoxProduct> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<DovetailDrawerBoxProduct>> Handle(Command command) {

            using var connection = await _factory.CreateConnection();
            connection.Open();
            var trx = connection.BeginTransaction();

            try {

                var box = CreateProduct(command);

                ProductsPersistance.InsertProduct(box, command.OrderId, connection, trx);

                trx.Commit();

                return box;

            } catch (Exception ex) {

                trx.Rollback();

                return new Error() {
                    Title = "Failed to Add Dovetail Drawer Box to Order",
                    Details = ex.Message
                };

            } finally {

                connection.Close();

            }

        }

        private static DovetailDrawerBoxProduct CreateProduct(Command command) {

            var id = Guid.NewGuid();
            var data = command.DrawerBox;

            var height = Dimension.FromMillimeters(data.Height);
            var width = Dimension.FromMillimeters(data.Width);
            var depth = Dimension.FromMillimeters(data.Depth);

            var labelFields = new Dictionary<string, string>();

            var options = new DovetailDrawerBoxConfig(data.BoxMaterial, data.BoxMaterial, data.BoxMaterial, data.BottomMaterial,
                                                data.Clips, data.Notches, data.Accessory, LogoPosition.None);

            var box = new DovetailDrawerBoxProduct(
                id, data.UnitPrice, data.Qty, data.Room, data.ProductNumber,
                height, width, depth, data.Note, labelFields, options);

            return box;

        }

    }

}
