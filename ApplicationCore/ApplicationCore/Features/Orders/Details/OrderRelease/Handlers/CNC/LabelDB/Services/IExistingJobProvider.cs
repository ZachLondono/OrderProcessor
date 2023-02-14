using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Contracts;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Services;

public interface IExistingJobProvider {

    public Task<ExistingJob> LoadExistingJobAsync(string source, string jobName);

}