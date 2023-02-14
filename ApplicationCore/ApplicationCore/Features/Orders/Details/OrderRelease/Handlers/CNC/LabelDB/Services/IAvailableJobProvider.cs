using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Contracts;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Services;

public interface IAvailableJobProvider {

    public Task<IEnumerable<AvailableJob>> GetAvailableJobsFromLabelFileAsync(string filePath);

}
