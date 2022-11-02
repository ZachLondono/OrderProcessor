using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.List;
public class GetCompanyList {

    public record Query() : IQuery<IEnumerable<CompanyListItem>>;

    public class Handler : QueryHandler<Query, IEnumerable<CompanyListItem>> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<CompanyListItem>>> Handle(Query request) {

            using var connection = _factory.CreateConnection();
            var query = "SELECT id, name FROM companies;";
            var items = await connection.QueryAsync<CompanyListItem>(query);

            return new(items);

        }
    }

}
