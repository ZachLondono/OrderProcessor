using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Queries;

public class GetCompanyNameById {

    public record Query(Guid CompanyId) : IQuery<string?>;

    public class Handler : QueryHandler<Query, string?> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<string?>> Handle(Query query) {

            const string queryString = @"SELECT
						                name
					                FROM
						                companies
					                WHERE id = @CompanyId;";

            using var connection = _factory.CreateConnection();

            string? name = await connection.QueryFirstOrDefaultAsync<string?>(queryString, query);

            return new(name);

        }

    }

}
