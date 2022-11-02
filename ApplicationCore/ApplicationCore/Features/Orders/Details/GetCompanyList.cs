using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Details;

public class GetCompanyList {

    public record Query() : IQuery<IEnumerable<CompanyListItem>>;

    public class Handler : QueryHandler<Query, IEnumerable<CompanyListItem>> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<CompanyListItem>>> Handle(Query request) {
            
            using var connection = _factory.CreateConnection();

            const string query = "SELECT id, name FROM companies;";

            var response = await connection.QueryAsync<CompanyListItem>(query);

            return new(response);

        }

    }

}
