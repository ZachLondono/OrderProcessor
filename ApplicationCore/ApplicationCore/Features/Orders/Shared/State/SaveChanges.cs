using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Orders.Shared.State;

public class SaveChanges {

    public record Command(Order Order) : ICommand;

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
                                note = @Note,
                                working_directory = @WorkingDirectory
                               WHERE id = @Id;
                               """;

            int rowsAffected = await connection.ExecuteAsync(sql, new {
                command.Order.Note,
                command.Order.WorkingDirectory,
                command.Order.Id
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
