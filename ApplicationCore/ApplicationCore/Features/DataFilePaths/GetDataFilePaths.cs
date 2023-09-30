using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Services;
using System.Text.Json;

namespace ApplicationCore.Features.DataFilePaths;

internal class GetDataFilePaths {

    public record Query(string FilePath) : IQuery<Shared.Settings.DataFilePaths>;

    public class Handler : QueryHandler<Query, Shared.Settings.DataFilePaths> {

        private readonly IFileReader _fileReader;

        public Handler(IFileReader fileReader) {
            _fileReader = fileReader;
        }

        public override async Task<Response<Shared.Settings.DataFilePaths>> Handle(Query query) {

            using var stream = _fileReader.OpenReadFileStream(query.FilePath);

            var data = await JsonSerializer.DeserializeAsync<DataFilePathsWrapper>(stream);

            if (data == null) {

                return new Error() {
                    Title = "Failed to load app configuration",
                    Details = "No value was read from configuration file."
                };

            }

            return data.FilePaths;

        }

    }

}
