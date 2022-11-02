using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Details;

public class GetOrdersByCompanyId {

    public record Query(Guid CompanyId) : IQuery<IEnumerable<CompanyOrderListItem>>;

    public class Handler : QueryHandler<Query, IEnumerable<CompanyOrderListItem>> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<CompanyOrderListItem>>> Handle(Query request) {
            
            using var connection = _factory.CreateConnection();

            const string query = "SELECT id, number, name FROM orders WHERE customerid = @CompanyId OR vendorid = @CompanyId;";

            var orders = await connection.QueryAsync<CompanyOrderListItem>(query, request);

            return new(orders);


        }

    }

}