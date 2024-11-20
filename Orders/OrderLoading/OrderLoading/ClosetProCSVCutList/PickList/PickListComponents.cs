using Domain.Orders.Entities.Hardware;
using Domain.ValueObjects;

namespace OrderLoading.ClosetProCSVCutList.PickList;

public record PickListComponents(Dimension HardwareSpread, Supply[] Supplies);