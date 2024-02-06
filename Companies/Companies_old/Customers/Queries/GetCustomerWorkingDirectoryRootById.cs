using ApplicationCore.Shared.Data.Companies;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Companies.Customers.Queries;

internal class GetCustomerWorkingDirectoryRootById {

    public record Query(Guid Id) : IQuery<string?>;

    public class Handler : QueryHandler<Query, string?> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<string?>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var data = await connection.QuerySingleOrDefaultAsync<string?>("SELECT working_directory_root FROM customers WHERE id = @Id;", query);

            return data;

        }

    }

}