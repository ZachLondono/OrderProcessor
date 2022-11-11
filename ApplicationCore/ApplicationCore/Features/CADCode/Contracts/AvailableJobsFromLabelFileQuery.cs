using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Contracts;

public record AvailableJobsFromLabelFileQuery(string LabelFilePath) : IQuery<IEnumerable<AvailableJob>>;
