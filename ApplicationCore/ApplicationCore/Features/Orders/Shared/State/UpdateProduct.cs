using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Orders.Shared.State;

public class UpdateProduct {

    public record Command(IProduct Product) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {
 
            using var connection = await _factory.CreateConnection();

            const string sql = """
                               UPDATE products
                               SET
                                qty = @Qty,
                                unit_price = @UnitPrice,
                                product_number = @ProductNumber,
                                room = @Room
                               WHERE id = @Id;
                               """;

            int rowsAffected = await connection.ExecuteAsync(sql, command.Product);

            if (rowsAffected < 1) {
                return Response.Error(new() {
                    Title = "Product Not Updated",
                    Details = "No rows in the database where affected when trying to update product. Product with given id may not exist."
                });
            }

            return Response.Success();

        }

    }

}
