using ApplicationCore.Features.CNC.ReleasePDF.Contracts;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.LabelDB;

public record ExistingJobFromLabelFileQuery(string LabelFilePath, string JobName) : IQuery<ReleasedJob>;