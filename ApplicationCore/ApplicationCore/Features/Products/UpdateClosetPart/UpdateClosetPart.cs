using Dapper;
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
                            comment = @Comment
                        WHERE product_id = @Id;
                        UPDATE products
                        SET
                            qty = @Qty,
                            room = @Room
                        WHERE id = @Id;
                        """;

            using var connection = await _factory.CreateConnection();

            int rows = await connection.ExecuteAsync(query, new {
                command.ClosetPart.Id,
                command.ClosetPart.SKU,
                command.ClosetPart.Comment,
                command.ClosetPart.Qty,
                command.ClosetPart.Room
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
