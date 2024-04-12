using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;
using Domain.Orders.Persistance.Repositories;

namespace ApplicationCore.Features.HardwareList.Commands;

public class DeleteDrawerSlide {

    public record Command(Guid SlideId) : ICommand;

    public class Handler(IOrderingDbConnectionFactory factory) : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory = factory;

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            var repo = new OrderDrawerSlidesRepository(connection);
            var wasInserted = await repo.DeleteDrawerSlide(command.SlideId);

            if (wasInserted) {

                return Response.Success();

            } else {

                return new Error() {
                    Title = "Failed to Delete Drawer Slide",
                    Details = "Drawer slide data could not be saved to database."
                };

            }

        }
    }

}
