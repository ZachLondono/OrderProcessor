using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CADCode.Contracts;

public record ReleaseCADCodeCSVBatchCommand(string FilePath, string CNCReportOutputDirectory) : ICommand;
