namespace OrderExporting.HardwareList;

public class Hardware {

	public string OrderNumber { get; set; } = string.Empty;
	public Supply[] Supplies { get; set; } = [];
	public HangingRod[] HangingRods { get; set; } = [];
	public DrawerSlide[] DrawerSlides { get; set; } = [];

}