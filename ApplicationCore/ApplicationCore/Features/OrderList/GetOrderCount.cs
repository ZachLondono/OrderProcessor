using Dapper;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.OrderList;

public class GetOrderCount {

    public record Query(Guid? CustomerId = null, Guid? VendorId = null, string? SearchTerm = null) : IQuery<int> {

        public string ModifiedSearchTerm => $"%{SearchTerm}%"; // % is a wildcard in SQLITE

    }

    public class Handler : QueryHandler<Query, int> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<int>> Handle(Query request) {

            using var connection = await _factory.CreateConnection();

            var queryFilter = new OrderListQueryFilterBuilder() {
                SearchTerm = request.SearchTerm,
                CustomerId = request.CustomerId,
                VendorId = request.VendorId,

            }.GetQueryFilter();

            var query = $"""
                        SELECT
                            COUNT(*) 
                        FROM orders{queryFilter};
                        """;

            var count = connection.QuerySingle<int>(query, request);

            return new(count);

        }

    }

}
