using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Orders.Details.Commands;

public class UpdateOrderNote {

    public record Command(Guid OrderId, string Note) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            const string sql = """
                               UPDATE orders
                               SET
                                note = @Note
                               WHERE id = @OrderId;
                               """;

            int rowsAffected = await connection.ExecuteAsync(sql, new {
                command.Note,
                command.OrderId
            });

            if (rowsAffected < 1) {
                return Response.Error(new() {
                    Title = "Order Not Updated",
                    Details = "No rows in the database where affected when trying to update order. Order with given id may not exist."
                });
            }

            return Response.Success();

        }

    }

}
