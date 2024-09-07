using Domain.Infrastructure.Bus;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Repositories;

namespace ApplicationCore.Features.HardwareList.Commands;

public class UpdateDrawerSlide {

    public record Command(DrawerSlide Slide) : ICommand;

    public class Handler(IOrderingDbConnectionFactory factory) : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory = factory;

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            var repo = new OrderDrawerSlidesRepository(connection);
            var wasInserted = await Task.Run(() => repo.UpdateDrawerSlide(command.Slide));

            if (wasInserted) {

                return Response.Success();

            } else {

                return new Error() {
                    Title = "Failed to Update Drawer Slide",
                    Details = "Drawer slide data could not be saved to database."
                };

            }

        }
    }

}