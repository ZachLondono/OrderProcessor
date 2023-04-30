using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Infrastructure.Bus;
using System.Text.Json;

namespace ApplicationCore.Features.Configuration;

internal class UpdateConfiguration {

    public record Command(string FilePath, AppConfiguration Configuration) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IFileWriter _fileWriter;

        public Handler(IFileWriter fileWriter) {
            _fileWriter = fileWriter;
        }

        public override async Task<Response> Handle(Command command) {

            var json = JsonSerializer.Serialize(command.Configuration, new JsonSerializerOptions() {
                WriteIndented = true
            });

            await _fileWriter.OverwriteWriteContentInFileAsync(command.FilePath, json);

            return Response.Success();


        }

    }

}
