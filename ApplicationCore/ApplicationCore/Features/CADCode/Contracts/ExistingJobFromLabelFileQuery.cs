using ApplicationCore.Features.CADCode.Contracts.ProgramRelease;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Contracts;

public record ExistingJobFromLabelFileQuery(string LabelFilePath, string JobName) : IQuery<ReleasedJob>;