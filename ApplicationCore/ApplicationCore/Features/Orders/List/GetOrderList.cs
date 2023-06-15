using ApplicationCore.Features.Shared.Data.Ordering;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Orders.List;

public class GetOrderList {

    public record Query(Guid? CustomerId = null, Guid? VendorId = null, string? SearchTerm = null, int Page = 0, int PageSize = 0) : IQuery<IEnumerable<OrderListItem>> {

        public string ModifiedSearchTerm => $"%{SearchTerm}%"; // % is a wildcard in SQLITE

        public int CurrentPageStart => (Page - 1) * PageSize;

    }

    public class Handler : QueryHandler<Query, IEnumerable<OrderListItem>> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<OrderListItem>>> Handle(Query request) {

            using var connection = await _factory.CreateConnection();

            var queryFilter = new OrderListQueryFilterBuilder() {
                SearchTerm = request.SearchTerm,
                CustomerId = request.CustomerId,
                VendorId = request.VendorId,
                Page = request.Page,
                PageSize = request.PageSize,
            }.GetQueryFilter();

            var query = $"""
                        SELECT
                            id,
                            name,
                            number,
                            order_date AS OrderDate,
                            customer_id AS CustomerId,
                            vendor_id AS VendorId,
                            (select SUM(qty) from products where products.order_id=orders.id) AS ItemCount
                        FROM orders{queryFilter};
                        """;

            var items = await connection.QueryAsync<OrderListItem>(query, request);

            return new(items);

        }

    }

}
