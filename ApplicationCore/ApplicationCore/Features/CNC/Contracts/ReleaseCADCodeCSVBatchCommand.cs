using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.Contracts;

public record ReleaseCADCodeCSVBatchCommand(string FilePath, string CNCReportOutputDirectory) : ICommand;
