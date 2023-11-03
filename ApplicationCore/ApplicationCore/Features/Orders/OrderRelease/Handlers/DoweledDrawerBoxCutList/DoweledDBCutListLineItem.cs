namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.DoweledDrawerBoxCutList;

public class DoweledDBCutListLineItem {

    public required string CabNumbers { get; set; }
    public required string PartName { get; set; }
    public required int Qty { get; set; }
    public required double Width { get; set; }
    public required double Length { get; set; }
    public required string Note { get; set; }

}
