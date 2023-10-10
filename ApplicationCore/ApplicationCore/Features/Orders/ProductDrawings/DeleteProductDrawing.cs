using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Ordering;
using Dapper;

namespace ApplicationCore.Features.Orders.ProductDrawings;

public class DeleteProductDrawing {

    public record Command(Guid DrawingId) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _connectionFactory;

        public Handler(IOrderingDbConnectionFactory connectionFactory) {
            _connectionFactory = connectionFactory;
        }

        public override async Task<Response> Handle(Command command) {

            try {

                using var connection = await _connectionFactory.CreateConnection();

                var result = await connection.ExecuteAsync("DELETE FROM product_drawings WHERE id = @DrawingId", command);

                if (result < 1) {

                    return new Error() {
                        Title = "Failed to Delete Product Drawing",
                        Details = "No rows where affected when trying to delete drawing."
                    };

                }

            } catch (Exception ex) {

                return Response.Error(new() {
                    Title = "Failed to Delete Product Drawing",
                    Details = ex.Message
                });

            }

            return Response.Success();

        }

    }

}
