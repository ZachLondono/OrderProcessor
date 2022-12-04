using ApplicationCore.Features.CNC.Contracts.ProgramRelease;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.ReleasePDF;

public record GenerateCNCReleasePDFRequest(ReleasedJob Job, string ReportOutputDirectory) : ICommand<IEnumerable<string>>;