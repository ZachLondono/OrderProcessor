using ApplicationCore.Features.WorkOrders.Data;
using ApplicationCore.Features.WorkOrders.Shared;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.WorkOrders.BarCodeScanning;

public class GetWorkOrderById {

    public record Query(Guid Id) : ICommand<WorkOrder>;

    public class Handler : CommandHandler<Query, WorkOrder> {

        public readonly IWorkOrdersDbConnectionFactory _factory;

        public Handler(IWorkOrdersDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<WorkOrder>> Handle(Query query) {

            using var connection = _factory.CreateConnection();

            DataModel? data = await connection.QuerySingleOrDefaultAsync<DataModel>(@"SELECT id, order_id AS OrderId, name, status FROM work_orders WHERE id = @Id", query);

            if (data is null) {
                return Response<WorkOrder>.Error(new() {
                    Title = "Not Found",
                    Details = "Work order was not found in database"
                });
            }

            var products = await connection.QueryAsync<Guid>(@"SELECT product_id FROM work_order_products WHERE work_order_id = @Id", data);

            var workorder = new WorkOrder(data.Id, data.Name, data.OrderId, products.ToList(), data.Status);

            return Response<WorkOrder>.Success(workorder);

        }

    }

    public class DataModel {

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Status Status { get; set; }

    }

}