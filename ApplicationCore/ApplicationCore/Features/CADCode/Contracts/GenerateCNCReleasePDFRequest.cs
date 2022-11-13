using ApplicationCore.Features.CADCode.Contracts.ProgramRelease;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Contracts;

public record GenerateCNCReleasePDFRequest(ReleasedJob Job, string ReportOutputDirectory) : ICommand<IEnumerable<string>>;