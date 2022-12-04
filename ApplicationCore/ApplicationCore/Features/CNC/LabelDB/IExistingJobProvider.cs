using ApplicationCore.Features.CNC.GCode.Contracts.ProgramRelease;

namespace ApplicationCore.Features.CNC.LabelDB;

public interface IExistingJobProvider
{

    public Task<ReleasedJob> LoadExistingJobAsync(string source, string jobName);

}