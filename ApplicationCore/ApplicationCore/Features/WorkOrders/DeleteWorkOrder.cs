using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.WorkOrders;

internal class DeleteWorkOrder {

    public record Command(Guid Id) : ICommand;

    public class Handler : CommandHandler<Command> {

        public readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = _factory.CreateConnection();
            connection.Open();
            var trx = connection.BeginTransaction();

            var rows = await connection.ExecuteAsync(@"DELETE FROM work_orders WHERE id = @Id; DELETE FROM work_order_products WHERE work_order_id = @Id;", command, trx);

            if (rows < 1) {

                trx.Rollback();
                connection.Close();

                return Response.Error(new() {
                    Title = "Cannot delete work order",
                    Details = "Could not delete work order from database"
                });

            }

            trx.Commit();
            connection.Close();
            return Response.Success();

        }

    }

}
