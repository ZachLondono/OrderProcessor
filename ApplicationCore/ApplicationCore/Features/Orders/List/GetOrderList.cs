using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.List;

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
                                    status,
                                    name,
                                    number,
                                    orderdate,
                                    customerid,
                                    (SELECT name FROM companies WHERE customerid = companies.id) as customername,
                                    vendorid,
                                    (SELECT name FROM companies WHERE vendorid = companies.id) as vendorname,
                                    (SELECT SUM(qty) FROM drawerboxes WHERE orderid = orders.id) as itemcount
                                FROM orders;";

            var items = await connection.QueryAsync<OrderListItem>(query);

            return new(items);

        }

    }

}
