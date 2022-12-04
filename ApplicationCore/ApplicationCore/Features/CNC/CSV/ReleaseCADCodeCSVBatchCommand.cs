using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.CSV;

public record ReleaseCADCodeCSVBatchCommand(string FilePath, string CNCReportOutputDirectory) : ICommand;
