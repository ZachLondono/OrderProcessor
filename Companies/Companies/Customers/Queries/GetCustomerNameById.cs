using Companies.Infrastructure;
using Domain.Infrastructure.Bus;

namespace Companies.Customers.Queries;

public class GetCustomerNameById {

    public record Query(Guid CustomerId) : IQuery<string>;

    public class Handler : QueryHandler<Query, string> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<string>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var name = await Task.Run(() => connection.QuerySingleOrDefault<string>("SELECT name FROM customers WHERE id = @CustomerId;", query));

            return name ?? "";

        }

    }

}

