using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;

namespace ApplicationCore.Features.Companies.Details;

public class GetOrdersByCompanyId {

    public record Query(Guid CompanyId) : IQuery<IEnumerable<CompanyOrderListItem>>;

    public class Handler : QueryHandler<Query, IEnumerable<CompanyOrderListItem>> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override Task<Response<IEnumerable<CompanyOrderListItem>>> Handle(Query request) {

            return Task.FromResult<Response<IEnumerable<CompanyOrderListItem>>>(new(new Error() {
                Title = "No Workey",
                Details = "This no workey no more"
            }));

        }

    }

}