using ApplicationCore.Features.CustomizationScripts.Models;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Ordering;
using Dapper;

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

            await connection.ExecuteAsync(
                """
                INSERT INTO order_customization_scripts
                (id, order_id, script_file_path, type)
                VALUES
                (@Id, @OrderId, @FilePath, @Type);
                """, command.Script);

            return Response.Success();

        }

    }

}
