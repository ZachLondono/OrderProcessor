using ApplicationCore.Features.CNC.LabelDB.Contracts;

namespace ApplicationCore.Features.CNC.LabelDB.Services;

public interface IExistingJobProvider {

    public Task<ExistingJob> LoadExistingJobAsync(string source, string jobName);

}