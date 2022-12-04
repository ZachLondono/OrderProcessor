using ApplicationCore.Features.CNC.Contracts;

namespace ApplicationCore.Features.CNC.Services;

public interface IAvailableJobProvider {

    public Task<IEnumerable<AvailableJob>> GetAvailableJobsFromLabelFileAsync(string filePath);

}
