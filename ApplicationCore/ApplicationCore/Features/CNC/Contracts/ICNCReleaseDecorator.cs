using ApplicationCore.Shared;

namespace ApplicationCore.Features.CNC.Contracts;

public interface ICNCReleaseDecorator : IDocumentDecorator {

    public Task<ReleasedJob?> LoadDataFromFile(string reportFilePath, DateTime orderDate, string customerName, string vendorName);

    //public void AddData(ReleasedJob job);

}
