using Domain.Infrastructure.Bus;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Enums;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Products;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.AddProductToOrder.AddFivePieceDoorToOrder;

public class InsertFivePieceDoorProductInOrder {

    public record Command(Guid OrderId, NewFivePieceDoorProduct Door) : ICommand<FivePieceDoorProduct>;

    public class Handler : CommandHandler<Command, FivePieceDoorProduct> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<FivePieceDoorProduct>> Handle(Command command) {

            using var connection = await _factory.CreateConnection();
            connection.Open();
            var trx = connection.BeginTransaction();

            try {

                var data = command.Door;

                Dimension width = Dimension.FromMillimeters(data.Width);
                Dimension height = Dimension.FromMillimeters(data.Height);
                DoorFrame frameSize = new(Dimension.FromMillimeters(data.Rails), Dimension.FromMillimeters(data.Stiles));
                Dimension frameThickness = Dimension.FromInches(0.75);
                Dimension panelThickness = Dimension.FromInches(0.25);

                var id = Guid.NewGuid();
                var door = new FivePieceDoorProduct(
                    id, data.Qty, data.UnitPrice, data.ProductNumber, data.Room,
                    width, height, frameSize,
                    frameThickness, panelThickness, data.Material,
                    DoorType.Door);

                ProductsPersistance.InsertProduct(door, command.OrderId, connection, trx);

                trx.Commit();

                return door;

            } catch (Exception e) {

                trx.Rollback();

                return new Error() {
                    Title = "Failed to Add Five Piece Door to Order",
                    Details = e.Message
                };

            } finally {

                connection.Close();

            }

        }

    }

}
