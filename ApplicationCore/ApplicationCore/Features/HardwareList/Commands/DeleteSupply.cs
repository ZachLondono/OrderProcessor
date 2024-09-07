using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Repositories;

namespace ApplicationCore.Features.HardwareList.Commands;

public class DeleteSupply {

    public record Command(Guid SupplyId) : ICommand;

    public class Handler(IOrderingDbConnectionFactory factory) : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory = factory;

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            var repo = new OrderSuppliesRepository(connection);
            var wasDeleted = await Task.Run(() => repo.DeleteSupply(command.SupplyId));

            if (wasDeleted) {

                return Response.Success();

            } else {

                return new Error() {
                    Title = "Failed to Delete Supply",
                    Details = "Supply data could not be saved to database."
                };

            }

        }
    }

}
