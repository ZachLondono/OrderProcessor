using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Features.Tools.Domain;
using ApplicationCore.Infrastructure.Bus;
using System.Text.Json;

namespace ApplicationCore.Features.Tools;

internal class GetTools {

    public record Query(string FilePath) : IQuery<IEnumerable<MachineToolMap>>;

    public class Handler : QueryHandler<Query, IEnumerable<MachineToolMap>> {

        private readonly IFileReader _fileReader;

        public Handler(IFileReader fileReader) {
            _fileReader = fileReader;
        }

        public override async Task<Response<IEnumerable<MachineToolMap>>> Handle(Query query) {

            using var stream = _fileReader.OpenReadFileStream(query.FilePath);

            var data = await JsonSerializer.DeserializeAsync<IEnumerable<MachineToolMap>>(stream);

            if (data is null) {

                return Response<IEnumerable<MachineToolMap>>.Error(new() {
                    Title = "Failed to read machine tool maps",
                    Details = "No value was returned while trying to read machine tool maps from file"
                });

            }

            return Response<IEnumerable<MachineToolMap>>.Success(data);


        }

    }

}
