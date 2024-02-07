namespace OrderLoading.ClosetProCSVCutList.CSVModels;

public record ClosetProOrderInfo(OrderHeader Header, List<Part> Parts, List<PickPart> PickList, List<Accessory> Accessories, List<BuyOutPart> BuyOutParts);
