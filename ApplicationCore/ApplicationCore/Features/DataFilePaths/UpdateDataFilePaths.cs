using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Services;
using System.Text.Json;

namespace ApplicationCore.Features.DataFilePaths;

internal class UpdateDataFilePaths {

    public record Command(string FilePath, Shared.Settings.DataFilePaths Configuration) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IFileWriter _fileWriter;

        public Handler(IFileWriter fileWriter) {
            _fileWriter = fileWriter;
        }

        public override async Task<Response> Handle(Command command) {

            var json = JsonSerializer.Serialize(new DataFilePathsWrapper(command.Configuration), new JsonSerializerOptions() {
                WriteIndented = true
            });

            await _fileWriter.OverwriteWriteContentInFileAsync(command.FilePath, json);

            return Response.Success();


        }

    }

}
