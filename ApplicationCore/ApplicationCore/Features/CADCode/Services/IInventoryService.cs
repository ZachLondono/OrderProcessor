using ApplicationCore.Features.CADCode.Services.Domain.Inventory;

namespace ApplicationCore.Features.CADCode.Services;

public interface IInventoryService {

    IEnumerable<InventoryItem> GetInventory();

}
