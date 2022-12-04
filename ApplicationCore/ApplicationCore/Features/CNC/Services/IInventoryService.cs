using ApplicationCore.Features.CNC.Services.Domain.Inventory;

namespace ApplicationCore.Features.CNC.Services;

public interface IInventoryService {

    Task<IEnumerable<InventoryItem>> GetInventory();

}
