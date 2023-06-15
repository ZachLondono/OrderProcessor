using ApplicationCore.Shared.Services;
using ApplicationCore.Features.CNC.Tools.Domain;
using ApplicationCore.Infrastructure.Bus;
using System.Text.Json;

namespace ApplicationCore.Features.CNC.Tools;

internal class SetTools {

    public record Command(string FilePath, IEnumerable<MachineToolMap> MachineToolMaps) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IFileWriter _fileWriter;

        public Handler(IFileWriter fileWriter) {
            _fileWriter = fileWriter;
        }

        public override async Task<Response> Handle(Command command) {

            var json = JsonSerializer.Serialize(command.MachineToolMaps, new JsonSerializerOptions() {
                WriteIndented = true
            });

            await _fileWriter.OverwriteWriteContentInFileAsync(command.FilePath, json);

            return Response.Success();

        }

    }

}
