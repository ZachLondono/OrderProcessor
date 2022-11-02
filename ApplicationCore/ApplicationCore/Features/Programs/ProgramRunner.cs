using ApplicationCore.Features.Programs.Contracts;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Programs;

public class ProgramRunner : QueryHandler<RunProgramRequest, RunProgramResponse> {

    private readonly ILogger<ProgramRunner> _logger;
    private readonly IProgramRunnerService _runnerService;

    public ProgramRunner(ILogger<ProgramRunner> logger, IProgramRunnerService runnerService) {
        _logger = logger;
        _runnerService = runnerService;
    }

    public override async Task<Response<RunProgramResponse>> Handle(RunProgramRequest request) {

        var exitCode = await _runnerService.RunProgramAsync(request.ExecutablePath, request.Arguments);

        var response = new RunProgramResponse(exitCode);

        return new(response);

    }
}