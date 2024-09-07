using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Repositories;

namespace ApplicationCore.Features.HardwareList.Commands;

public class DeleteHangingRail {

    public record Command(Guid RailId) : ICommand;

    public class Handler(IOrderingDbConnectionFactory factory) : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory = factory;

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            var repo = new OrderHangingRailRepository(connection);
            var wasInserted = await Task.Run(() => repo.DeleteHangingRail(command.RailId));

            if (wasInserted) {

                return Response.Success();

            } else {

                return new Error() {
                    Title = "Failed to Delete Hanging Rail",
                    Details = "Hanging rail data could not be saved to database."
                };

            }

        }
    }

}
