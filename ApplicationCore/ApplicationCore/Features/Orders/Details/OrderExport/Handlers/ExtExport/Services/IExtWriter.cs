using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Domain;

namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Services;

public interface IExtWriter {
    void AddRecord(JobDescriptor job);
    void AddRecord(LevelDescriptor level);
    void AddRecord(ProductRecord product);
    void AddRecord(LevelVariableOverride variables);
    void WriteFile(string filePath);
    void Clear();
}