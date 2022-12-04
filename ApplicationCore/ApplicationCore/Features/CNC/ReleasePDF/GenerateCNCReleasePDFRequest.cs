using ApplicationCore.Features.CNC.GCode.Contracts.ProgramRelease;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.ReleasePDF;

public record GenerateCNCReleasePDFRequest(ReleasedJob Job, string ReportOutputDirectory) : ICommand<IEnumerable<string>>;