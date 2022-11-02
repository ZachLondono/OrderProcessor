namespace ApplicationCore.Features.Programs;

public interface IProgramRunnerService {

    public Task<int> RunProgramAsync(string executablePath, string args);

}
