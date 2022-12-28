using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.ProductPlanner.Services;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.ProductPlanner;
public class GenerateEXTFile {

    public record Command(PSIJob Job,string FilePath) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly ExtWriter _writer;

        public Handler(ExtWriter writer) {
            _writer = writer;
        }

        public override Task<Response> Handle(Command command) {

            _writer.AddRecord(command.Job.Job);

            foreach (var variables in command.Job.VariableOverrides) {
                _writer.AddRecord(variables);
            }

            foreach (var level in command.Job.Levels) {
                _writer.AddRecord(level);
            }

            foreach (var product in command.Job.Products) {
                _writer.AddRecord(product);
            }

            _writer.WriteFile(command.FilePath);

            return Task.FromResult(new Response());

        }

    }
}
