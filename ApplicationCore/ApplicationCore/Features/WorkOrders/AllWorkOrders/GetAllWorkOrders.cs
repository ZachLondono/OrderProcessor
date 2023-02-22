using ApplicationCore.Features.Shared.Contracts;
using ApplicationCore.Features.WorkOrders.Shared;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.WorkOrders.AllWorkOrders;

public class GetAllWorkOrders {

    public record Query() : ICommand<IEnumerable<Model>>;

    public class Handler : CommandHandler<Query, IEnumerable<Model>> {

        private readonly IDbConnectionFactory _factory;
        private readonly Ordering.GetOrderNumberById _getOrderNumberById;

        public Handler(IDbConnectionFactory factory, Ordering.GetOrderNumberById getOrderNumberById) {
            _factory = factory;
            _getOrderNumberById = getOrderNumberById;
        }

        public override async Task<Response<IEnumerable<Model>>> Handle(Query command) {

            using var connection = _factory.CreateConnection();

            var workorders = await connection.QueryAsync<Model>(@"SELECT id, order_id As OrderId, name, status FROM work_orders");

            foreach (var wo in workorders) {

                wo.OrderNumber = await _getOrderNumberById(wo.OrderId);

            }

            return Response<IEnumerable<Model>>.Success(workorders);

        }

    }

    public class Model {

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Status Status { get; set; }
        public string OrderNumber { get; set; } = string.Empty;

    }

}