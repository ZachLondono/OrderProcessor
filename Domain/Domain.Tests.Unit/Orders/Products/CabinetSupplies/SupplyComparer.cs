using Domain.Orders.Entities.Hardware;

namespace Domain.Tests.Unit.Orders.Products.CabinetSupplies;

internal static class SupplyComparer {

    public static bool Compare(Supply a, Supply b) {

        return a.Qty == b.Qty && a.Description == b.Description;

    }

}
