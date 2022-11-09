using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Queries;

public class GetDrawerBoxMaterialById {

    public record Query(Guid MaterialId) : IQuery<DrawerBoxMaterial?>;

    public class Handler : QueryHandler<Query, DrawerBoxMaterial?> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<DrawerBoxMaterial?>> Handle(Query request) {

            using var connection = _factory.CreateConnection();

            const string query = "SELECT id, name, thickness_mm FROM drawerboxmaterials WHERE id = @MaterialId;";

            var option = await connection.QuerySingleOrDefaultAsync<DrawerBoxMaterial>(query, request);

            return new(option);

        }
    }
}