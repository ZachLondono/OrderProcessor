using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Configuration;

internal class GetConfiguration {

    public record Query() : IQuery<AppConfiguration>;

    public class Handler : QueryHandler<Query, AppConfiguration> {

        private readonly IConfigurationDBConnectionFactory _factory;
        
        public Handler(IConfigurationDBConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<AppConfiguration>> Handle(Query query) {
            
            using var connection = await _factory.CreateConnection();

            var config = await connection.QuerySingleAsync<AppConfiguration>(
                """
                SELECT
                    ordering_db_path AS OrderingDBPath,
                    companies_db_path AS CompaniesDBPath,
                    work_orders_db_path AS WorkOrdersDBPath
                FROM configuration;
                """);

            if (config is null) {
                return Response<AppConfiguration>.Error(new() {
                    Title = "Configuration Not Found",
                    Details = "Configuration details where not found"
                });
            }

            return Response<AppConfiguration>.Success(config);

        }

    }

}
