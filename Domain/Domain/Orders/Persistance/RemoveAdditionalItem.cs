using Dapper;
using Domain.Infrastructure.Bus;

namespace Domain.Orders.Persistance;

public class RemoveAdditionalItem {

    public record Command(Guid ItemId) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            const string sql =
                """
                DELETE FROM additional_items WHERE id = @ItemId;
                """;

            int rowsAffected = connection.Execute(sql, command);

            if (rowsAffected < 1) {

                return Response.Error(new() {
                    Title = "Item Was Not Deleted",
                    Details = "No rows in the database where affected while trying to delete item. Item with given id may not exist."
                });

            }

            return Response.Success();

        }

    }

}
