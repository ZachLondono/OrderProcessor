using Domain.Infrastructure.Bus;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.Products.UpdateClosetPart;

public class UpdateClosetPart {

    public record Command(ClosetPart ClosetPart) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            var query = """
                        UPDATE closet_parts
                        SET
                            sku = @SKU,
                            width = @Width,
                            length = @Length,
                            material_finish = @MaterialFinish,
                            material_core = @MaterialCore,
                            paint_color = @PaintColor,
                            painted_side = @PaintedSide,
                            edge_banding_finish = @EdgeBandingFinish,
                            comment = @Comment,
                            parameters = @Parameters,
                            install_cams = @InstallCams
                        WHERE product_id = @Id;
                        UPDATE products
                        SET
                            qty = @Qty,
                            room = @Room,
                            product_number = @ProductNumber,
                            unit_price = @UnitPrice,
                            production_notes = @ProductionNotes
                        WHERE id = @Id;
                        """;

            using var connection = await _factory.CreateConnection();

            int rows = connection.Execute(query, new {
                command.ClosetPart.Id,
                command.ClosetPart.SKU,
                command.ClosetPart.Width,
                command.ClosetPart.Length,
                MaterialFinish = command.ClosetPart.Material.Finish,
                MaterialCore = command.ClosetPart.Material.Core,
                PaintColor = (command.ClosetPart.Paint?.Color ?? null),
                PaintedSide = (command.ClosetPart.Paint?.Side ?? 0),
                EdgeBandingFinish = command.ClosetPart.EdgeBandingColor,
                command.ClosetPart.Comment,
                Parameters = (IDictionary<string, string>) command.ClosetPart.Parameters,
                command.ClosetPart.InstallCams,
                command.ClosetPart.Qty,
                command.ClosetPart.Room,
                command.ClosetPart.UnitPrice,
                command.ClosetPart.ProductNumber,
                command.ClosetPart.ProductionNotes,
            });

            if (rows != 2) {

                return new Error() {
                    Title = "Error Updating Record",
                    Details = "Received an unexpected response from database when attempting to update closet part records"
                };

            }

            return Response.Success();

        }

    }

}
