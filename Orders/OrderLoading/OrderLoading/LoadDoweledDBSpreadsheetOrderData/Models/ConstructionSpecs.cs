using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public record ConstructionSpecs {

	public bool MachineFrontFromOutside { get; set; }
	public bool MachineBackFromOutside { get; set; }
	public double FrontBackDrop { get; set; }
	public double DrillingDepthAdjustment { get; set; }
	public bool FullHeightFront { get; set; }
	public bool FullHeightBack { get; set; }

	public static ConstructionSpecs ReadFromSheet(Worksheet worksheet) {

		return new() {
			MachineFrontFromOutside = worksheet.GetRangeStringValue("MachineFrontFromOutside") == "Yes",
			MachineBackFromOutside = worksheet.GetRangeStringValue("MachineBackFromOutside") == "Yes",
			FrontBackDrop = worksheet.GetRangeValue<double>("FrontBackAdj", 0),
			DrillingDepthAdjustment = worksheet.GetRangeValue<double>("DrillDepthAdjustment", 0.0),
			FullHeightFront = worksheet.GetRangeStringValue("FullHeightFront") == "Yes",
			FullHeightBack = worksheet.GetRangeStringValue("FullHeightBack") == "Yes"
		};

	}

}


