namespace ApplicationCore.Features.ClosetProCSVCutList.CSVModels;

public class BuyOutPart {
    public int WallNum { get; set; }
    public int SectionNum { get; set; }
    public string PartType { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string ExportName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public double Height { get; set; }
    public double Width { get; set; }
    public double Depth { get; set; }
    public int Quantity { get; set; }
    public string VertHand { get; set; } = string.Empty;
    public double VertDrillL { get; set; }
    public double VertDrillR { get; set; }
    public double BBHeight { get; set; }
    public double BBDepth { get; set; }
    public double ShoeHeight { get; set; }
    public double ShoeDepth { get; set; }
    public string DrillLeft1 { get; set; } = string.Empty;
    public string DrillLeft2 { get; set; } = string.Empty;
    public string DrillRight1 { get; set; } = string.Empty;
    public string DrillRight2 { get; set; } = string.Empty;
    public string RailNotch { get; set; } = string.Empty;
    public double RailNotchElevation { get; set; }
    public string CornerShelfSizes { get; set; } = string.Empty;
    public string PartCost { get; set; } = string.Empty;
    public string UnitL { get; set; } = string.Empty;
    public string UnitR { get; set; } = string.Empty;
    public int PartNum { get; set; }

    public List<PartInfo> InfoRecords = new();

}
