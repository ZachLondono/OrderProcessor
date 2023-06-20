using ApplicationCore.Infrastructure.Bus;
using CADCodeProxy.Machining;
using CADCodeProxy.CSV;

namespace ApplicationCore.Features.CSVTokens;

public class WriteTokensToCSV {

    public record Command(Batch Batch, string OutputDirectory) : ICommand<string>;

    public class Handler : CommandHandler<Command, string> {

        public override Task<Response<string>> Handle(Command command) {

            var writer = new CSVTokenWriter();

            var filePath = writer.WriteBatchCSV(command.Batch, command.OutputDirectory);

            return Task.FromResult(Response<string>.Success(filePath));

        }

    }

}
