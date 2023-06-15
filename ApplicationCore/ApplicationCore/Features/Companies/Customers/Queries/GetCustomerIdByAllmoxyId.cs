using ApplicationCore.Shared.Data.Companies;
using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Companies.Customers.Queries;

internal class GetCustomerIdByAllmoxyId {

    public record Query(int AllmoxyId) : IQuery<Guid?>;

    public class Handler : QueryHandler<Query, Guid?> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Guid?>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var data = await connection.QuerySingleOrDefaultAsync<Guid?>("SELECT customer_id FROM allmoxy_ids WHERE id = @AllmoxyId;", query);

            return Response<Guid?>.Success(data);

        }

    }

}