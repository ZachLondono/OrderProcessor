using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.WorkOrders;

internal class CreateWorkOrder {

    public record Command(string Name, Guid OrderId, IReadOnlyCollection<Guid> ProductIds) : ICommand<WorkOrder>;

    public class Handler : CommandHandler<Command, WorkOrder> {

        public readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<WorkOrder>> Handle(Command command) {

            using var connection = _factory.CreateConnection();
            connection.Open();
            var trx = connection.BeginTransaction();

            WorkOrder workorder = new(Guid.NewGuid(), command.Name, command.OrderId, command.ProductIds, Status.Pending);

            var rows = await connection.ExecuteAsync(@"INSERT INTO work_orders (id, name, order_id, status) VALUES (@Id, @Name, @OrderId, @Status);", workorder, trx);

            if (rows != 1) {

                trx.Rollback();
                connection.Close();

                return Response<WorkOrder>.Error(new() {
                    Title = "Cannot create work order",
                    Details = "Could not insert work order into database"
                });

            }

            foreach (var productId in command.ProductIds) {

                rows = await connection.ExecuteAsync("INSERT INTO work_order_products (work_order_id, product_id) VALUES (@WorkOrderId, @ProductId)", new { WorkOrderId = workorder.Id, ProductId = productId }, trx);

                if (rows != 1) {

                    trx.Rollback();
                    connection.Close();

                    return Response<WorkOrder>.Error(new() {
                        Title = "Cannot create work order",
                        Details = "Could not add product to work order"
                    });

                }

            }

            trx.Commit();
            connection.Close();
            return Response<WorkOrder>.Success(workorder);

        }

    }

}
