using Domain.Infrastructure.Bus;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Repositories;

namespace ApplicationCore.Features.HardwareList.Commands;

public class UpdateSupply {

    public record Command(Supply Supply) : ICommand;

    public class Handler(IOrderingDbConnectionFactory factory) : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory = factory;

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            var repo = new OrderSuppliesRepository(connection);
            var wasInserted = repo.UpdateSupply(command.Supply);

            if (wasInserted) {

                return Response.Success();

            } else {

                return new Error() {
                    Title = "Failed to Update Supply",
                    Details = "Supply data could not be saved to database."
                };

            }

        }
    }

}