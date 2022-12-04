namespace ApplicationCore.Features.CNC.LabelDB;

public interface IAvailableJobProvider
{

    public Task<IEnumerable<AvailableJob>> GetAvailableJobsFromLabelFileAsync(string filePath);

}
