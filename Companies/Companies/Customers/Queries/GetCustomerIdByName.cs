using Companies.Infrastructure;
using Dapper;
using Domain.Infrastructure.Bus;

namespace Companies.Customers.Queries;

public class GetCustomerIdByName {

    public record Query(string Name) : IQuery<Guid?>;

    public class Handler : QueryHandler<Query, Guid?> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Guid?>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var data = await connection.QuerySingleOrDefaultAsync<Guid?>("SELECT id FROM customers WHERE name = @Name;", query);

            return data;

        }

    }

}