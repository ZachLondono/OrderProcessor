using ApplicationCore.Features.CNC.Contracts.ProgramRelease;

namespace ApplicationCore.Features.CNC.Services;

public interface IExistingJobProvider {

    public Task<ReleasedJob> LoadExistingJobAsync(string source, string jobName);

}