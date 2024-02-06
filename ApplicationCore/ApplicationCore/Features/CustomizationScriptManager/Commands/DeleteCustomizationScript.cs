using Dapper;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.CustomizationScripts.Commands;

internal class DeleteCustomizationScript {

    public record Command(Guid Id) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            await connection.ExecuteAsync(
                """
                DELETE FROM order_customization_scripts
                WHERE id = @Id;
                """, command);

            return Response.Success();

        }

    }

}
