using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared;

namespace ApplicationCore.Features.Orders.WorkingDirectory;

internal class MigrateWorkingDirectory {

    public record Command(string OldWorkingDirectory, string NewWorkingDirectory, bool CopyFiles, bool DeleteFiles) : ICommand;

    public class Handler : CommandHandler<Command> {
        
        public override async Task<Response> Handle(Command command) {

            if (!Directory.Exists(command.NewWorkingDirectory)) {
                Directory.CreateDirectory(command.NewWorkingDirectory);
            }

            if (Directory.Exists(command.OldWorkingDirectory)) {

                Action<string, string> operation;
                if (command.CopyFiles && command.DeleteFiles) {
                    operation = File.Move;
                } else if (command.CopyFiles) {
                    operation = File.Copy;
                } else if (command.DeleteFiles) {
                    operation = (a, b) => File.Delete(a);
                } else {
                    return Response.Success();
                }

                var files = Directory.GetFiles(command.OldWorkingDirectory, "*", SearchOption.TopDirectoryOnly);

                if (files.Length > 10) {
                    return Response.Error(new() {
                        Title = "Refused to Migrate Directories",
                        Details = "Existing directory contains too many files, please migrate directories manually."
                    });
                }

                await Task.Run(() => {

                   files.Select(file => (file, Path.Combine(command.NewWorkingDirectory, Path.GetFileName(file))))
                        .ForEach(files => operation(files.file, files.Item2));

                    if (Directory.GetFiles(command.OldWorkingDirectory).Length == 0) {
                        Directory.Delete(command.OldWorkingDirectory);
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
