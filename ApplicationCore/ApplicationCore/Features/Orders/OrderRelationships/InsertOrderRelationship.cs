using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Ordering;
using Dapper;

namespace ApplicationCore.Features.Orders.OrderRelationships;

internal class InsertOrderRelationship {

    public record Command(Guid Order1Id, Guid Order2Id) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            await connection.ExecuteAsync(
                """
                INSERT INTO order_relationships (order_1_id, order_2_id) VALUES (@Order1Id, @Order2Id);
                """, 
                command);

            return Response.Success();

        }

    }

}
