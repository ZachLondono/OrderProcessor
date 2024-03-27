using Domain.Infrastructure.Bus;

namespace ApplicationCore.Features.GetJobCutListDirectory;

internal class GetJobOrderCutListDirectory {

    public record Query(string OrderFileDirectory, string DefaultOutputDirectory) : IQuery<string>;

    public class Handler : QueryHandler<Query, string> {

        public override Task<Response<string>> Handle(Query query) {

            if (!query.OrderFileDirectory.StartsWith(@"R:\Job Scans")) {
                return Task.FromResult(Response<string>.Success(query.DefaultOutputDirectory));
            }

            if (!query.OrderFileDirectory.Contains("orders", StringComparison.InvariantCultureIgnoreCase) && !query.OrderFileDirectory.Contains("work progress", StringComparison.InvariantCultureIgnoreCase)) {
                return Task.FromResult(Response<string>.Success(Path.Combine(query.OrderFileDirectory, "CUTLIST")));
            }

            string directory = query.OrderFileDirectory;
            while (true) {

                var dirInfo = new DirectoryInfo(directory);

                if (dirInfo.Parent is null) {
                    break;
                }

                if (Path.GetFileNameWithoutExtension(directory) is string dirName) {

                    if (dirName.Equals("orders", StringComparison.InvariantCultureIgnoreCase) || dirName.Equals("work progress", StringComparison.InvariantCultureIgnoreCase)) {

                        var parentDi = new DirectoryInfo(dirInfo.Parent.FullName);
                        var existingDir = parentDi.GetDirectories()
                                                .Where(info => info.Name.Contains("cutlist", StringComparison.InvariantCultureIgnoreCase))
                                                .FirstOrDefault();

                        if (existingDir is not null) {
                            return Task.FromResult(Response<string>.Success(existingDir.FullName));
                        }

                        return Task.FromResult(Response<string>.Success(Path.Combine(dirInfo.Parent.FullName, "CUTLIST")));

                    }

                } else {
                    break;
                }

                directory = dirInfo.Parent.FullName;

            }

            var defaultCutListDir = Path.Combine(query.OrderFileDirectory, "CUTLIST");

            if (Directory.Exists(defaultCutListDir)) {
                return Task.FromResult(Response<string>.Success(defaultCutListDir));
            }

            return Task.FromResult(Response<string>.Success(query.OrderFileDirectory));

        }

    }

}
