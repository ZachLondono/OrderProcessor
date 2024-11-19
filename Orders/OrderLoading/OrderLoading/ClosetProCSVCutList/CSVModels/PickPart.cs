namespace OrderLoading.ClosetProCSVCutList.CSVModels;

public class PickPart {
	public string Type { get; set; } = string.Empty;
	public string PartName { get; set; } = string.Empty;
	public string ExportName { get; set; } = string.Empty;
	public string Color { get; set; } = string.Empty;
	public double Height { get; set; }
	public double Width { get; set; }
	public double Depth { get; set; }
	public int Quantity { get; set; }
	public string Cost { get; set; } = string.Empty;
}
