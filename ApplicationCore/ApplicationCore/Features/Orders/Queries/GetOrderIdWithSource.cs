using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Queries;

public class GetOrderIdWithSource {

    public record Query(string Source) : IQuery<Guid?>;

    public class Handler : QueryHandler<Query, Guid?> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Guid?>> Handle(Query request) {

            using var connection = _factory.CreateConnection();

            const string existQuery = "SELECT id FROM orders WHERE source = @Source LIMIT 1;";
            var result = await connection.QuerySingleOrDefaultAsync<Guid?>(existQuery, request);

            return new(result);

        }

    }

}
