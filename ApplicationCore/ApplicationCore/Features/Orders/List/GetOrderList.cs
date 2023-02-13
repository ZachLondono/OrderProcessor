using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.List;

public class GetOrderList {

    public record Query() : IQuery<IEnumerable<OrderListItem>>;

    public class Handler : QueryHandler<Query, IEnumerable<OrderListItem>> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<OrderListItem>>> Handle(Query request) {

            using var connection = _factory.CreateConnection();

            const string query = @"SELECT
                                    id,
                                    name,
                                    number,
                                    orderdate,
                                    customername,
                                    vendorid,
                                    0 as itemcount
                                FROM orders;";

            // TODO: get item count

            var items = await connection.QueryAsync<OrderListItem>(query);

            return new(items);

        }

    }

}
