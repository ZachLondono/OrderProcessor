using ApplicationCore.Features.WorkOrders.Data;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.WorkOrders.Shared.Commands;

internal class UpdateWorkOrder {

    public record Command(Guid Id, string Name, Status Status) : ICommand;

    public class Handler : CommandHandler<Command> {

        public readonly IWorkOrdersDbConnectionFactory _factory;

        public Handler(IWorkOrdersDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();
            connection.Open();
            var trx = connection.BeginTransaction();

            var rows = await connection.ExecuteAsync(@"UPDATE work_orders SET name = @Name, status = @Status WHERE id = @Id;", command, trx);

            if (rows != 1) {

                trx.Rollback();
                connection.Close();

                return Response.Error(new() {
                    Title = "Cannot update work order",
                    Details = "Could not update work order in database"
                });

            }

            trx.Commit();
            connection.Close();
            return Response.Success();

        }

    }

}
