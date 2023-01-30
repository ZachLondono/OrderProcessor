using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.ProductPlanner.Services;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.ProductPlanner;
public class GenerateEXTFile {

    public record Command(PPJob Job, string FilePath) : ICommand;

    public class Handler : CommandHandler<Command> {

        public override Task<Response> Handle(Command command) {

            var writer = new ExtWriter();

            new PPJobConverter(writer).ConvertOrder(command.Job);

            writer.WriteFile(command.FilePath);

            return Task.FromResult(new Response());

        }

    }
}
