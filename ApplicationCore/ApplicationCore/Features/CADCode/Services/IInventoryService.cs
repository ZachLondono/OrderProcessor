using ApplicationCore.Features.CADCode.Services.Domain.Inventory;

namespace ApplicationCore.Features.CADCode.Services;

public interface IInventoryService {

    Task<IEnumerable<InventoryItem>> GetInventory();

}
