using ApplicationCore.Features.CNC.LabelDB.Contracts;

namespace ApplicationCore.Features.CNC.LabelDB.Services;

public interface IAvailableJobProvider {

    public Task<IEnumerable<AvailableJob>> GetAvailableJobsFromLabelFileAsync(string filePath);

}
