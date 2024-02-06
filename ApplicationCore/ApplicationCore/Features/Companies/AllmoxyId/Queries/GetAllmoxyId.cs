using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Companies;
using Dapper;

namespace Domain.Companies.AllmoxyId.Queries;

internal class GetAllmoxyId {

    public record Query(Guid CustomerId) : IQuery<int?>;

    public class Handler : QueryHandler<Query, int?> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<int?>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            int? allmoxyId = await connection.QueryFirstOrDefaultAsync<int?>("SELECT id FROM allmoxy_ids WHERE customer_id = @CustomerId;", query);

            return allmoxyId;

        }
    }

}
