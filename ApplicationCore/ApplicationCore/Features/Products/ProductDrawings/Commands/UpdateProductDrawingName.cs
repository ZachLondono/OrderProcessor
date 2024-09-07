using Dapper;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.Orders.ProductDrawings.Commands;

public class UpdateProductDrawingName {

    public record Command(Guid DrawingId, string DrawingName) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _connectionFactory;

        public Handler(IOrderingDbConnectionFactory connectionFactory) {
            _connectionFactory = connectionFactory;
        }
        public override async Task<Response> Handle(Command command) {

            using var connection = await _connectionFactory.CreateConnection();

            connection.Execute(
                """
                UPDATE product_drawings
                    SET name = @DrawingName
                WHERE id = @DrawingId;
                """, command);

            return Response.Success();

        }
    }

}
