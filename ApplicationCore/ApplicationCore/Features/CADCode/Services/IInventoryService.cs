using ApplicationCore.Features.CADCode.Services.Domain.Inventory;

namespace ApplicationCore.Features.CADCode.Services;

internal interface IInventoryService {

    IEnumerable<InventoryItem> GetInventory();

}
