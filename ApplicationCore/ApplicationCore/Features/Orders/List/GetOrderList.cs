using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Orders.List;

public class GetOrderList {

    public record Query(Guid? CustomerId, Guid? VendorId) : IQuery<IEnumerable<OrderListItem>>;

    public class Handler : QueryHandler<Query, IEnumerable<OrderListItem>> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<OrderListItem>>> Handle(Query request) {

            using var connection = await _factory.CreateConnection();

            List<string> filters = new();
            if (request.VendorId is not null) {
                filters.Add("vendor_id = @VendorId");
            }
            if (request.CustomerId is not null) {
                filters.Add("customer_id = @CustomerId");
            }

            string filter = "";
            if (filters.Any()) {
                filter += " WHERE " + string.Join(" AND ", filters.ToArray());
            }

            string query = $@"SELECT
                                id,
                                name,
                                number,
                                order_date AS OrderDate,
                                customer_id AS CustomerId,
                                vendor_id AS VendorId,
                                (select SUM(qty) from products where products.order_id=orders.id) AS ItemCount
                            FROM orders{filter};";

            var items = await connection.QueryAsync<OrderListItem>(query, request);

            return new(items);

        }

    }

}
