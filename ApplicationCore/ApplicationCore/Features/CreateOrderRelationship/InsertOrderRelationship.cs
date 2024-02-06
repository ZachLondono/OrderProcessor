using Dapper;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.CreateOrderRelationship;

internal class InsertOrderRelationship {

    public record Command(Guid Order1Id, Guid Order2Id) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<int>(
                """
                SELECT EXISTS (SELECT 1 FROM order_relationships WHERE (order_1_id = @Order1Id AND order_2_id = @Order2Id) OR (order_1_id = @Order2Id AND order_2_id = @Order1Id)); 
                """,
                command);

            if (result is int n && n == 1) {
                // Relationship already exists
                return Response.Success();
            }

            await connection.ExecuteAsync(
                """
                INSERT INTO order_relationships (order_1_id, order_2_id) VALUES (@Order1Id, @Order2Id);
                """,
                command);

            return Response.Success();

        }

    }

}
