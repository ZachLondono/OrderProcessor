using Domain.ValueObjects;

namespace OrderExporting.HardwareList;

public class DrawerSlide {

	public int Qty { get; set; }
	public Dimension Length { get; set; }
	public string Style { get; set; } = string.Empty;

}
