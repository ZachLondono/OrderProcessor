using ApplicationCore.Shared.Data.Companies;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Companies.Customers.Queries;

internal class GetCustomerNameById {

    public record Query(Guid CustomerId) : IQuery<string>;

    public class Handler : QueryHandler<Query, string> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<string>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var name = await connection.QuerySingleAsync<string>("SELECT name FROM customers WHERE id = @CustomerId;", query);

            return name ?? "";

        }

    }

}

