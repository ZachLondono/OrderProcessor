using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public record UMSlideSpecs {

	public bool UMSlideMachining { get; set; }
	public double UMSlidesFromSide { get; set; }
	public bool UMSlideHook { get; set; }
	public double UMSlideHookFromEdge { get; set; }
	public double UMSlideHookFromBottom { get; set; }
	public double UMSlideHook5mmBoreDepth { get; set; }
	public double UMSlideHook8mmBoreDepth { get; set; }
	public bool UMSlideNotches { get; set; }
	public double UMSlideNotchHeight { get; set; }
	public double UMSlideNotchWidth { get; set; }
	public bool ResizeForUMSlides { get; set; }
	public double ResizeAmount { get; set; }
	public bool MachineFrontsForClips { get; set; }
	public double DistanceFromClipsToFace { get; set; }

	public static UMSlideSpecs ReadFromSheet(Worksheet worksheet) {

		return new() {
			UMSlideMachining = worksheet.GetRangeStringValue("MachineSidesForSlides") == "Yes",
			UMSlidesFromSide = worksheet.GetRangeValue<double>("SlidesFromSides", 0),
			UMSlideHook = worksheet.GetRangeStringValue("BoreForHook") == "Yes",
			UMSlideHookFromEdge = worksheet.GetRangeValue<double>("HookFromEdge", 0),
			UMSlideHookFromBottom = worksheet.GetRangeValue<double>("HookFromBottom", 0),
			UMSlideHook5mmBoreDepth = worksheet.GetRangeValue<double>("Hook5mmDepth", 0),
			UMSlideHook8mmBoreDepth = worksheet.GetRangeValue<double>("Hook8mmDepth", 0),
			UMSlideNotches = worksheet.GetRangeStringValue("CutNotches") == "Yes",
			UMSlideNotchHeight = worksheet.GetRangeValue<double>("NotchHeight", 0),
			UMSlideNotchWidth = worksheet.GetRangeValue<double>("NotchWidth", 0),
			ResizeForUMSlides = worksheet.GetRangeStringValue("ResizeForUMSlides") == "Yes",
			ResizeAmount = worksheet.GetRangeValue<double>("ResizeAmountForUMSlides", 0),
			MachineFrontsForClips = worksheet.GetRangeStringValue("MachineFrontsForClips") == "Yes",
			DistanceFromClipsToFace = worksheet.GetRangeValue<double>("ClipsFromFace", 0)
		};
	}

}


