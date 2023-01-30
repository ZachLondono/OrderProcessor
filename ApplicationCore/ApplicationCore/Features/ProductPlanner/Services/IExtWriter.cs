using ApplicationCore.Features.ProductPlanner.Domain;

namespace ApplicationCore.Features.ProductPlanner.Services;

public interface IExtWriter {
    void AddRecord(JobDescriptor job);
    void AddRecord(LevelDescriptor level);
    void AddRecord(ProductRecord product);
    void AddRecord(VariableOverride variables);
    void WriteFile(string filePath);
    void Clear();
}