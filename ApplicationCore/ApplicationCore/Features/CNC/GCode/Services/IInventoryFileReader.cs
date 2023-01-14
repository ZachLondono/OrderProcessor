using ApplicationCore.Features.CNC.GCode.Domain;

namespace ApplicationCore.Features.CNC.GCode.Services;

public interface IInventoryFileReader {
    Task<IEnumerable<InventorySheetStock>> GetAvailableInventoryAsync(string filePath);
}
