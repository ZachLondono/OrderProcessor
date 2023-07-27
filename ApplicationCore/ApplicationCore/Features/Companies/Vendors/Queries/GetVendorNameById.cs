using ApplicationCore.Shared.Data.Companies;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Companies.Vendors.Queries;

internal class GetVendorNameById {

    public record Query(Guid VendorId) : IQuery<string>;

    public class Handler : QueryHandler<Query, string> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<string>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var name = await connection.QuerySingleAsync<string>("SELECT name FROM vendors WHERE id = @VendorId;", query);

            return Response<string>.Success(name);

        }

    }

}
