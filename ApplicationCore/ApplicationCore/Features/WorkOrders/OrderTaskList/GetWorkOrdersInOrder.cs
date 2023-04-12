using ApplicationCore.Features.WorkOrders.Data;
using ApplicationCore.Features.WorkOrders.Shared;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.WorkOrders.OrderTaskList;

internal class GetWorkOrdersInOrder {

    public record Query(Guid OrderId) : ICommand<IEnumerable<WorkOrder>>;

    public class Handler : CommandHandler<Query, IEnumerable<WorkOrder>> {

        public readonly IWorkOrdersDbConnectionFactory _factory;

        public Handler(IWorkOrdersDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<WorkOrder>>> Handle(Query command) {

            using var connection = await _factory.CreateConnection();

            var data = await connection.QueryAsync<DataModel>(@"SELECT id, name, status FROM work_orders WHERE order_id = @OrderId", command);

            List<WorkOrder> workorders = new();

            foreach (var wo in data) {

                var products = await connection.QueryAsync<Guid>(@"SELECT product_id FROM work_order_products WHERE work_order_id = @Id", wo);

                var workorder = new WorkOrder(wo.Id, wo.Name, command.OrderId, products.ToList(), wo.Status);

                workorders.Add(workorder);

            }

            return Response<IEnumerable<WorkOrder>>.Success(workorders);

        }

    }

    public class DataModel {

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Status Status { get; set; }

    }

}
