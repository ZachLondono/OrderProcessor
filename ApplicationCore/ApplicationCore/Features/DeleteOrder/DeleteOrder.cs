using Dapper;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.DeleteOrder;

public class DeleteOrder {

    public record Command(Guid OrderId) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            int rows = await connection.ExecuteAsync("DELETE FROM orders WHERE id = @OrderId;", command);

            if (rows > 0) {
                return Response.Success();
            } else {
                return Response.Error(new() {
                    Title = "Order Not Deleted",
                    Details = "No records where affected when trying to delete order. The order may have not exist or the delete operation failed."
                });
            }

        }

    }

}
