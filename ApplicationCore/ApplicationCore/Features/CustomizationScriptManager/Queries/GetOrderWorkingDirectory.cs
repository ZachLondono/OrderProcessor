using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.CustomizationScriptList.Queries;

public class GetOrderWorkingDirectory {

    public record Query(Guid OrderId) : IQuery<string>;

    public class Handler : QueryHandler<Query, string> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<string>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var workingDirectory = await Task.Run(() => connection.QuerySingleOrDefault<string>(
                """
                SELECT 
                    working_directory
                FROM orders
                WHERE id = @OrderId;
                """, query));

            if (workingDirectory is null) {

                return new Error() {
                    Title = "Working Directory Not Found",
                    Details = "Could not find working directory for order."
                };

            }

            return workingDirectory;

        }
    }

}
