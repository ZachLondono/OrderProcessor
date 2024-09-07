using ApplicationCore.Shared.CustomizationScripts.Models;
using Dapper;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.CustomizationScripts.Commands;

internal class AddCustomizationScript {

    public record Command(CustomizationScript Script) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            connection.Execute(
                """
                INSERT INTO order_customization_scripts
                (id, order_id, name, script_file_path, type)
                VALUES
                (@Id, @OrderId, @Name, @FilePath, @Type);
                """, command.Script);

            return Response.Success();

        }

    }

}
