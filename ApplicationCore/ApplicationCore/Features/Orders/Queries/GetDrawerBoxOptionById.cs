using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Queries;

public class GetDrawerBoxOptionById {

    public record Query(Guid OptionId) : IQuery<DrawerBoxOption?>;

    public class Handler : QueryHandler<Query, DrawerBoxOption?> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<DrawerBoxOption?>> Handle(Query request) {

            using var connection = _factory.CreateConnection();

            const string query = "SELECT id, name FROM drawerboxoptions WHERE id = @OptionId;";

            var option = await connection.QuerySingleOrDefaultAsync<DrawerBoxOption>(query, request);

            return new(option);

        }
    }
}