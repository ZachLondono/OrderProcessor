using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ApplicationCore.Features.Programs;

internal class ProgramRunnerService : IProgramRunnerService {

    private readonly ILogger<ProgramRunnerService> _logger;

    public ProgramRunnerService(ILogger<ProgramRunnerService> logger) {
        _logger = logger;
    }

    public async Task<int> RunProgramAsync(string executablePath, string args) {

        _logger.LogInformation("Running program");

        var start = new ProcessStartInfo {
            Arguments = args,
            FileName = executablePath,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true
        };

        using Process? proc = Process.Start(start);
        if (proc is null) {
            _logger.LogError("Could not start process at file path {FilePath} with args {Args}", executablePath, args);
            return -1;
        }
        await proc.WaitForExitAsync();

        return proc.ExitCode;

    }

}