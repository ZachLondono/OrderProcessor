using Dapper;
using Domain.Infrastructure.Bus;

namespace Domain.Orders.Persistance;

public class UpdateOrderWorkingDirectory {

    public record Command(Guid OrderId, string WorkingDirectory) : ICommand;

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
                                working_directory = @WorkingDirectory
                               WHERE id = @OrderId;
                               """;

            int rowsAffected = connection.Execute(sql, new {
                command.WorkingDirectory,
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
