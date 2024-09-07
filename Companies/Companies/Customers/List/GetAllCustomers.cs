using Companies.Infrastructure;
using Dapper;
using Domain.Infrastructure.Bus;

namespace Companies.Customers.List;

public class GetAllCustomers {

    public record Query() : IQuery<IEnumerable<CustomerListItem>>;

    public class Handler : QueryHandler<Query, IEnumerable<CustomerListItem>> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<CustomerListItem>>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var customers = connection.Query<CustomerListItem>("SELECT id, NAME FROM customers;");

            return Response<IEnumerable<CustomerListItem>>.Success(customers);

        }

    }

}