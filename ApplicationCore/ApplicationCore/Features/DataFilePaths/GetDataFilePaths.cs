using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Infrastructure.Bus;
using System.Text.Json;

namespace ApplicationCore.Features.Configuration;

internal class GetDataFilePaths {

    public record Query(string FilePath) : IQuery<DataFilePaths>;

    public class Handler : QueryHandler<Query, DataFilePaths> {

        private readonly IFileReader _fileReader;

        public Handler(IFileReader fileReader) {
            _fileReader = fileReader;
        }

        public override async Task<Response<DataFilePaths>> Handle(Query query) {

            using var stream = _fileReader.OpenReadFileStream(query.FilePath);

            var data = await JsonSerializer.DeserializeAsync<DataFilePaths>(stream);

            if (data == null) {

                return Response<DataFilePaths>.Error(new() {
                    Title = "Failed to load app configuration",
                    Details = "No value was read from configuration file."
                });

            }

            return Response<DataFilePaths>.Success(data);

        }

    }

}
