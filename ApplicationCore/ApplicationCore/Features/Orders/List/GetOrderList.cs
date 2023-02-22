using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Orders.List;

public class GetOrderList {

    public record Query() : IQuery<IEnumerable<OrderListItem>>;

    public class Handler : QueryHandler<Query, IEnumerable<OrderListItem>> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<OrderListItem>>> Handle(Query request) {

            using var connection = _factory.CreateConnection();

            const string query = @"SELECT
                                    id,
                                    name,
                                    number,
                                    order_date AS OrderDate,
                                    customer_name AS CustomerName,
                                    vendor_id AS VendorId,
                                    0 as itemcount
                                FROM orders;";

            // TODO: get item count

            var items = await connection.QueryAsync<OrderListItem>(query);

            return new(items);

        }

    }

}
