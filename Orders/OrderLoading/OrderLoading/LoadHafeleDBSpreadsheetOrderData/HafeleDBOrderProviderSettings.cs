using Microsoft.Extensions.Configuration;

namespace OrderLoading.LoadHafeleDBSpreadsheetOrderData;

public class HafeleDBOrderProviderSettings {

	[ConfigurationKeyName("vendor_id")]
	public Guid VendorId { get; set; }

	[ConfigurationKeyName("material_thicknesses_mm")]
	public Dictionary<string, double> MaterialThicknessesMM { get; set; } = new();

	[ConfigurationKeyName("working_directory_root")]
	public string WorkingDirectoryRoot { get; set; } = string.Empty;

	[ConfigurationKeyName("front_back_height_adj_mm")]
	public double FrontBackHeightAdjMM { get; set; }

}
