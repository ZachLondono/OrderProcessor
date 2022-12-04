using ApplicationCore.Features.CNC.ReleasePDF.Contracts;

namespace ApplicationCore.Features.CNC.LabelDB;

public interface IExistingJobProvider
{

    public Task<ReleasedJob> LoadExistingJobAsync(string source, string jobName);

}