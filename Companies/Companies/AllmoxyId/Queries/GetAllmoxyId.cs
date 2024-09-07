using Companies.Infrastructure;
using Dapper;
using Domain.Infrastructure.Bus;

namespace Companies.AllmoxyId.Queries;

public class GetAllmoxyId {

    public record Query(Guid CustomerId) : IQuery<int?>;

    public class Handler : QueryHandler<Query, int?> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<int?>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            int? allmoxyId = connection.QueryFirstOrDefault<int?>("SELECT id FROM allmoxy_ids WHERE customer_id = @CustomerId;", query);

            return allmoxyId;

        }
    }

}
