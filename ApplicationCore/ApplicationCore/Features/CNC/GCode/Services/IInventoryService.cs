using ApplicationCore.Features.CNC.GCode.Domain.Inventory;

namespace ApplicationCore.Features.CNC.GCode.Services;

public interface IInventoryService
{

    Task<IEnumerable<InventoryItem>> GetInventory();

}
