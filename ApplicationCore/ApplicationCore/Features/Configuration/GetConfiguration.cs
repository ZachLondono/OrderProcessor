using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Infrastructure.Bus;
using System.Text.Json;

namespace ApplicationCore.Features.Configuration;

internal class GetConfiguration {

    public record Query(string FilePath) : IQuery<AppConfiguration>;

    public class Handler : QueryHandler<Query, AppConfiguration> {

        private readonly IFileReader _fileReader;

        public Handler(IFileReader fileReader) {
            _fileReader = fileReader;
        }

        public override async Task<Response<AppConfiguration>> Handle(Query query) {

            using var stream = _fileReader.OpenReadFileStream(query.FilePath);

            var data = await JsonSerializer.DeserializeAsync<AppConfiguration>(stream);

            if (data == null) {

                return Response<AppConfiguration>.Error(new() {
                    Title = "Failed to load app configuration",
                    Details = "No value was read from configuration file."
                });

            }

            return Response<AppConfiguration>.Success(data);

        }

    }

}
