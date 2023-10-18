using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Ordering;
using Dapper;

namespace ApplicationCore.Features.Orders.Shared.State;

public class UpdateAdditionalItem {

    public record Command(AdditionalItem Item) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            const string sql =
                """
                UPDATE additional_items
                SET description = @Description, price = @Price, is_service = @IsService
                WHERE id = @Id;
                """;

            int rowsAffected = await connection.ExecuteAsync(sql, new {
                command.Item.Description,
                command.Item.Price,
                command.Item.IsService,
                command.Item.Id
            });

            if (rowsAffected < 1) {

                return Response.Error(new() {
                    Title = "Item Was Not Updated",
                    Details = "No rows in the database where affected while trying to update item. Item with given id may not exist."
                });

            }

            return Response.Success();

        }

    }

}
