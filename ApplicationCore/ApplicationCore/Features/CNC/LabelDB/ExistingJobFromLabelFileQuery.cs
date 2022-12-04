using ApplicationCore.Features.CNC.GCode.Contracts.ProgramRelease;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.LabelDB;

public record ExistingJobFromLabelFileQuery(string LabelFilePath, string JobName) : IQuery<ReleasedJob>;