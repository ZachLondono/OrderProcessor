using ApplicationCore.Features.CNC.CSV.Contracts;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.CSV;

public class ReadTokensFromCSVFile {

	public record Command(string FilePath) : ICommand<CSVReadResult>;

	public class Handler : CommandHandler<Command, CSVReadResult> {

		private readonly ICSVReader _reader;

		public Handler(ICSVReader reader) {
			_reader = reader;
		}

		public override async Task<Response<CSVReadResult>> Handle(Command command) {
			
			try {

				var readResult = await _reader.ReadTokensFromFilesAsync(command.FilePath);

				return new(readResult);

			} catch (Exception ex) {

				return new(new Error() {
					Title = "Exception thrown while reading CSV tokens",
					Details = ex.ToString()
				});

			}

		}

	}

}