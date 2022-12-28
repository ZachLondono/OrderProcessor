using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.ProductPlanner.Services;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.ProductPlanner;
public class GenerateEXTFile {

    public record Command(JobDescriptor Job, IEnumerable<VariableOverride> VariableOverrides, IEnumerable<LevelDescriptor> Levels, IEnumerable<Product> Products, string FilePath) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly ExtWriter _writer;

        public Handler(ExtWriter writer) {
            _writer = writer;
        }

        public override Task<Response> Handle(Command command) {

            _writer.AddRecord(command.Job);
            foreach (var variables in command.VariableOverrides) {
                _writer.AddRecord(variables);
            }

            foreach (var level in command.Levels) {
                _writer.AddRecord(level);
            }

            foreach (var product in command.Products) {
                _writer.AddRecord(product);
            }

            _writer.WriteFile(command.FilePath);

            return Task.FromResult(new Response());

        }

    }
}
