using ApplicationCore.Infrastructure.Bus;
using Dapper;

namespace ApplicationCore.Features.Configuration;

internal class UpdateConfiguration {

    public record Command(AppConfiguration Configuration) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IConfigurationDBConnectionFactory _factory;

        public Handler(IConfigurationDBConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            var rowsAffected = await connection.ExecuteAsync(
                """
                UPDATE configuration 
                SET 
                    ordering_db_path = @OrderingDBPath,
                    companies_db_path = @CompaniesDBPath,
                    work_orders_db_path = @WorkOrdersDBPath
                WHERE id = 1;
                """, command.Configuration);

            if (rowsAffected == 0) {
                return Response.Error(new() {
                    Title = "Configuration Not Updated",
                    Details = "No data was changed while trying to update configuration"
                });
            }

            return Response.Success();

        }

    }

}
