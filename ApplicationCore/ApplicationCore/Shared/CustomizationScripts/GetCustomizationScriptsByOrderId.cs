using ApplicationCore.Shared.CustomizationScripts.Models;
using Dapper;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Shared.CustomizationScripts;

internal class GetCustomizationScriptsByOrderId {

    public record Query(Guid OrderId) : IQuery<IEnumerable<CustomizationScript>>;

    public class Handler : QueryHandler<Query, IEnumerable<CustomizationScript>> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<CustomizationScript>>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var scripts = connection.Query<CustomizationScript>(
                """
                SELECT
                    id,
                    order_id AS OrderId,
                    name,
                    script_file_path AS FilePath,
                    type
                FROM order_customization_scripts
                WHERE order_id = @OrderId;
                """, query);

            return Response<IEnumerable<CustomizationScript>>.Success(scripts);

        }

    }

}
