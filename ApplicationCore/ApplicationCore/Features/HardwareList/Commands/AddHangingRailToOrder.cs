using Domain.Infrastructure.Bus;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Repositories;

namespace ApplicationCore.Features.HardwareList.Commands;

public class AddHangingRailToOrder {

    public record Command(Guid OrderId, HangingRail HangingRail) : ICommand;

    public class Handler(IOrderingDbConnectionFactory factory) : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory = factory;

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            var repo = new OrderHangingRailRepository(connection);
            var wasInserted = repo.AddHangingRailToOrder(command.OrderId, command.HangingRail);

            if (wasInserted) {

                return Response.Success();

            } else {

                return new Error() {
                    Title = "Failed to Add Hanging Rail to Order",
                    Details = "Hanging rail data could not be saved to database."
                };

            }

        }

    }

}
