using ApplicationCore.Features.CustomizationScripts.Models;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Ordering;
using Dapper;

namespace ApplicationCore.Features.CustomizationScripts.Queries;

internal class GetCustomizationScriptsByOrderId {

    public record Query(Guid OrderId) : IQuery<IEnumerable<CustomizationScript>>;

    public class Handler : QueryHandler<Query, IEnumerable<CustomizationScript>> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<CustomizationScript>>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var scripts = await connection.QueryAsync<CustomizationScript>(
                """
                SELECT
                    id,
                    order_id AS OrderId,
                    script_file_path AS FilePath,
                    type
                FROM order_customization_scripts
                WHERE order_id = @OrderId;
                """, query);

            return (Response<IEnumerable<CustomizationScript>>)scripts;

        }

    }

}
