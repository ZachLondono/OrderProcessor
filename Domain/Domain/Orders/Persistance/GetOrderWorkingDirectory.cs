using Dapper;
using Domain.Infrastructure.Bus;

namespace Domain.Orders.Persistance;

public class GetOrderWorkingDirectory {

    public record Query(Guid OrderId) : IQuery<string>;

    public class Handler : QueryHandler<Query, string> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }


        public override async Task<Response<string>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            return await connection.QuerySingleAsync<string>(
                """
                SELECT 
                    working_directory
                FROM orders
                WHERE id = @OrderId;
                """,
                query);

        }

    }

}
