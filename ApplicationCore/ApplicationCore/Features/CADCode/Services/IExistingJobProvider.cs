using ApplicationCore.Features.CADCode.Contracts.ProgramRelease;

namespace ApplicationCore.Features.CADCode.Services;

public interface IExistingJobProvider {

    public Task<ReleasedJob> LoadExistingJobAsync(string source, string jobName);

}