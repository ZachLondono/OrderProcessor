using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Orders.List;

public class GetOrderList {

    public record Query(Guid? CustomerId = null, Guid? VendorId = null, string? SearchTerm = null) : IQuery<IEnumerable<OrderListItem>> {
        public string ModifiedSearchTerm => $"%{SearchTerm}%"; // % is a wildcard in SQLITE
    }

    public class Handler : QueryHandler<Query, IEnumerable<OrderListItem>> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<OrderListItem>>> Handle(Query request) {

            using var connection = await _factory.CreateConnection();

            List<string> filters = new();
            if (request.VendorId is not null && request.VendorId != Guid.Empty) {
                filters.Add("vendor_id = @VendorId");
            }
            if (request.CustomerId is not null && request.CustomerId != Guid.Empty) {
                filters.Add("customer_id = @CustomerId");
            }
            if (request.SearchTerm is not null) {
                filters.Add($"(name LIKE @ModifiedSearchTerm OR number LIKE @ModifiedSearchTerm)");
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
                            FROM orders{filter}
                            ORDER BY order_date DESC;";

            var items = await connection.QueryAsync<OrderListItem>(query, request);

            return new(items);

        }

    }

}
