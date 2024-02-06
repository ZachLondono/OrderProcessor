using ApplicationCore.Shared.Data.Companies;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Companies.Vendors.List;

public class GetAllVendors {

    public record Query() : IQuery<IEnumerable<VendorListItem>>;

    public class Handler : QueryHandler<Query, IEnumerable<VendorListItem>> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<VendorListItem>>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var vendors = await connection.QueryAsync<VendorListItem>("SELECT id, NAME FROM vendors;");

            return Response<IEnumerable<VendorListItem>>.Success(vendors);

        }

    }

}