using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;
using System.Data;
using System.Diagnostics;

namespace ApplicationCore.Features.Orders.Commands;

public class CreateNewOrder {

    public record Command(string Source, string Number, string Name, Guid CustomerId, Guid VendorId, string Comment, DateTime OrderDate, decimal Tax, decimal Shipping, decimal PriceAdjustment, IReadOnlyDictionary<string,string> Info, IEnumerable<DrawerBox> Boxes, IEnumerable<AdditionalItem> AdditionalItems, Guid? OrderId = null) : ICommand<Order>;

    public class Handler : CommandHandler<Command, Order> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }
        
        public override async Task<Response<Order>> Handle(Command request) {

            Order order = Order.Create(request.Source, request.Number, request.Name, request.CustomerId, request.VendorId, request.Comment, request.OrderDate, request.Tax, request.Shipping, request.PriceAdjustment, request.Info, request.Boxes, request.AdditionalItems, request.OrderId);

            using var connection = _factory.CreateConnection();

            connection.Open();
            var trx = connection.BeginTransaction();

            try {

                const string command = @"INSERT INTO orders (id, source, status, number, name, customerid, vendorid, productionnote, customercomment, orderdate, releasedate, productiondate, completedate, info, tax, shipping, priceadjustment)
                                        VALUES (@Id, @Source, @Status, @Number, @Name, @CustomerId, @VendorId, @ProductionNote, @CustomerComment, @OrderDate, @ReleaseDate, @ProductionDate, @CompleteDate, @Info, @Tax, @Shipping, @PriceAdjustment);";

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
                    Info = (IEnumerable<KeyValuePair<string, string>>) order.Info,
                    order.Tax,
                    order.Shipping,
                    order.PriceAdjustment
                }, trx);

                foreach (var box in request.Boxes) {

                    await CreateDrawerBox(box, order.Id, connection, trx);

                }

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

        private static async Task CreateDrawerBox(DrawerBox box, Guid orderId, IDbConnection connection, IDbTransaction transaction) {

            const string boxCommand = @"INSERT INTO drawerboxes (id, orderid, lineinorder, unitprice, qty, height_mm, width_mm, depth_mm, note, postfinish, scoopfront, logo, facemountingholes, boxmaterialid, bottommaterialid, clipsid, notchesid, accessoryid, uboxdimensions, fixeddividers)
                                        VALUES (@Id, @OrderId, @LineInOrder, @UnitPrice, @Qty, @Height, @Width, @Depth, @Note, @PostFinish, @ScoopFront, @Logo, @FaceMountingHoles, @BoxMaterialId, @BottomMaterialId, @ClipsId, @NotchesId, @AccessoryId, @UBoxDimensions, @FixedDivdersCounts);";

            await connection.ExecuteAsync(boxCommand, new {
                box.Id,
                OrderId = orderId,
                box.LineInOrder,
                box.UnitPrice,
                box.Qty,
                box.Height,
                box.Width,
                box.Depth,
                box.Note,
                box.Options.PostFinish,
                box.Options.ScoopFront,
                box.Options.Logo,
                box.Options.FaceMountingHoles,
                BoxMaterialId =  box.Options.BoxMaterial.Id,
                BottomMaterialId = box.Options.BottomMaterial.Id,
                ClipsId = box.Options.Clips.Id,
                NotchesId = box.Options.Notches.Id,
                AccessoryId = box.Options.Accessory.Id,
                box.Options.UBoxDimensions,
                box.Options.FixedDivdersCounts
            }, transaction);

        }

    }

}
