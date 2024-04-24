using Domain.ValueObjects;

namespace OrderExporting.HardwareList;

public class HangingRod {

	public int Qty { get; set; }
	public Dimension Length { get; set; }
	public string Finish { get; set; } = string.Empty;

}
