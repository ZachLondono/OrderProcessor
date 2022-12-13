using ApplicationCore.Features.CNC.GCode.Domain;

namespace ApplicationCore.Features.CNC.GCode.Services;

public interface IToolFileReader {
	Task<IEnumerable<Tool>> GetAvailableToolsAsync(string filePath);
}
