namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;

public record ClosetProOrderInfo(OrderHeader Header, List<Part> Parts, List<PickPart> PickList, List<Accessory> Accessories);
