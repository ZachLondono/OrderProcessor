using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.WorkingDirectory;

internal class MigrateWorkingDirectory {

    public record Command(string OldWorkingDirectory, string NewWorkingDirectory, bool CopyFiles, bool DeleteFiles) : ICommand;

    public class Handler : CommandHandler<Command> {
        
        public override Task<Response> Handle(Command command) {
            throw new NotImplementedException();
        }

    }


}
