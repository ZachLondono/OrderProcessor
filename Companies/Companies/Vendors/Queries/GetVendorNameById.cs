using Companies.Infrastructure;
using Dapper;
using Domain.Infrastructure.Bus;

namespace Companies.Vendors.Queries;

public class GetVendorNameById {

    public record Query(Guid VendorId) : IQuery<string>;

    public class Handler : QueryHandler<Query, string> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<string>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var name = connection.QuerySingle<string>("SELECT name FROM vendors WHERE id = @VendorId;", query);

            return name ?? "";

        }

    }

}
