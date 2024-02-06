using ApplicationCore.Shared;
using Domain.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.Details.Models.WorkingDirectory;

internal class MigrateWorkingDirectory {

    public record Command(string OldWorkingDirectory, string NewWorkingDirectory, MigrationType MigrationType) : ICommand;

    public class Handler : CommandHandler<Command> {

        public const int MAX_FILES = 10;

        private readonly IFileHandler _fileHandler;

        public Handler(IFileHandler fileHandler) {
            _fileHandler = fileHandler;
        }

        public override async Task<Response> Handle(Command command) {

            if (!_fileHandler.DirectoryExists(command.NewWorkingDirectory)) {
                _fileHandler.CreateDirectory(command.NewWorkingDirectory);
            }

            if (command.MigrationType == MigrationType.None) {
                return Response.Success();
            }

            if (_fileHandler.DirectoryExists(command.OldWorkingDirectory)) {

                Action<string, string> operation = command.MigrationType switch {
                    MigrationType.CopyFiles => _fileHandler.Copy,
                    MigrationType.MoveFiles => _fileHandler.Move,
                    MigrationType.DeleteFiles => (a, b) => _fileHandler.DeleteFile(a),
                    _ => throw new InvalidOperationException("Unexpected migration type")
                };

                var files = _fileHandler.GetFiles(command.OldWorkingDirectory, "*", SearchOption.TopDirectoryOnly);

                if (files.Length > MAX_FILES) {
                    return Response.Error(new() {
                        Title = "Refused to Migrate Directories",
                        Details = "Existing directory contains too many files, please migrate directories manually."
                    });
                }

                await Task.Run(() => {

                    files.Select(file => (file, Path.Combine(command.NewWorkingDirectory, Path.GetFileName(file))))
                         .ForEach(files => operation(files.file, files.Item2));

                    if (_fileHandler.GetFiles(command.OldWorkingDirectory, "*", SearchOption.AllDirectories).Length == 0) {
                        _fileHandler.DeleteFile(command.OldWorkingDirectory);
                    }

                });

                return Response.Success();

            } else {

                return Response.Error(new() {
                    Title = "Could not Migrate Directories",
                    Details = "The old working directory does not exist"
                });

            }

        }

    }

}
