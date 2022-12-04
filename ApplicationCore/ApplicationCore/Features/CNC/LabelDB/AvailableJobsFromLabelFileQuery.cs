using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.LabelDB;

public record AvailableJobsFromLabelFileQuery(string LabelFilePath) : IQuery<IEnumerable<AvailableJob>>;
