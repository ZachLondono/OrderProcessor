using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Orders.List;

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
                            COUNT(id) 
                        FROM orders{queryFilter};
                        """;

            var count = await connection.QuerySingleAsync<int>(query, request);

            return new(count);

        }

    }

}
