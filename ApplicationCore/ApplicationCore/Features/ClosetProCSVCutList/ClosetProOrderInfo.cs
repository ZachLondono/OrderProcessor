namespace ApplicationCore.Features.ClosetProCSVCutList;

public record ClosetProOrderInfo(OrderHeader Header, List<Part> Parts, List<PickPart> PickList, List<Accessory> Accessories, List<BuyOutPart> BuyOutParts);
