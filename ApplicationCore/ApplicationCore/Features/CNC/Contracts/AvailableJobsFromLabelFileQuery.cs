using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.Contracts;

public record AvailableJobsFromLabelFileQuery(string LabelFilePath) : IQuery<IEnumerable<AvailableJob>>;
