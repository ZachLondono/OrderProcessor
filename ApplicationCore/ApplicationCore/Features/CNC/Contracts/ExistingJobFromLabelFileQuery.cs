using ApplicationCore.Features.CNC.Contracts.ProgramRelease;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.Contracts;

public record ExistingJobFromLabelFileQuery(string LabelFilePath, string JobName) : IQuery<ReleasedJob>;