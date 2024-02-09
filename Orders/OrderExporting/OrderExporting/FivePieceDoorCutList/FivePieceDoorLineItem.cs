namespace OrderExporting.FivePieceDoorCutList;

public class FivePieceDoorLineItem {

    public int CabNumber { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Qty { get; set; }
    public double Width { get; set; }
    public double Length { get; set; }
    public string Note { get; set; } = string.Empty;

}
