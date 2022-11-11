using ApplicationCore.Features.CADCode.Contracts;

namespace ApplicationCore.Features.CADCode.Services;

public interface IAvailableJobProvider {

    public Task<IEnumerable<AvailableJob>> GetAvailableJobsFromLabelFileAsync(string filePath);

}
