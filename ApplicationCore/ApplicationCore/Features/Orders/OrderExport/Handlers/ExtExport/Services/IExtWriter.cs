using Domain.ProductPlanner;

namespace ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Services;

public interface IExtWriter {
    void AddRecord(JobDescriptor job);
    void AddRecord(LevelDescriptor level);
    void AddRecord(ProductRecord product);
    void AddRecord(LevelVariableOverride variables);
    string WriteFile(string outputDirectory, string fileName);
    void Clear();
}