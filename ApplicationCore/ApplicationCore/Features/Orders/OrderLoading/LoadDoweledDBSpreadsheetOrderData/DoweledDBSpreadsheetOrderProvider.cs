using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData;

internal class DoweledDBSpreadsheetOrderProvider : IOrderProvider {

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    public DoweledDBSpreadsheetOrderProvider() {

    }

    public Task<OrderData?> LoadOrderData(string source) {
        throw new NotImplementedException();
    }

    private record Header(DateTime OrderDate, DateTime DueDate, string OrderNumber, string OrderName, string ConnectorType, string Construction, string Units);

    private record LineItem {

        public int Number { get; init; }
        public string Note { get; init; } = string.Empty;
        public int Qty { get; init; }
        public double Height { get; init; }
        public double Width { get; init; }
        public double Depth { get; init; }
        public string Instructions { get; init; } = string.Empty;
        public string FrontBackColor { get; init; } = string.Empty;
        public double FrontBackThickness { get; init; }
        public bool FrontBackGrained { get; init; }
        public string SidesColor { get; init; } = string.Empty;
        public double SidesThickness { get; init; }
        public bool SidesGrained { get; init; }
        public string BottomBackColor { get; init; } = string.Empty;
        public double BottomBackThickness { get; init; }
        public bool BottomBackGrained { get; init; }

        public static LineItem ReadFromSheet(Worksheet worksheet, int row) {
            throw new NotImplementedException();
        }

    }

    private record CustomerInfo {

        public string Contact { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Line1 { get; init; } = string.Empty;
        public string Line2 { get; init; } = string.Empty;
        public string Line3 { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string Zip { get; init; } = string.Empty;

        public static CustomerInfo ReadFromSheet(Worksheet worksheet) {
            throw new NotImplementedException();
        }

    }

    private record UMSlideSpecs {

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
            throw new NotImplementedException();
        }

    }

    private record BottomSpecs {

        public double BottomDadoOversize { get; set; }
        public double BottomDadoDepth { get; set; }
        public double BottomHeight { get; set; }
        public double BottomDadoOvercut { get; set; }
        public double BottomDadoToolDiameter { get; set; }
        public double BottomSizeAdjustment { get; set; }
        public bool PreDrillBottoms { get; set; }
        public double MinPreDrillingSpace { get; set; }

        public static BottomSpecs ReadFromSheet(Worksheet worksheet) {
            throw new NotImplementedException();
        }

    }

    private record FrontDrillingSpecs {

        public bool PeanutSlotFronts { get; set; }
        public double PeanutSlotFrontOffSides { get; set; }
        public bool DrillFronts { get; set; }
        public double FrontDrillingOffSides { get; set; }

        public static FrontDrillingSpecs ReadFromSheet(Worksheet worksheet) {
            throw new NotImplementedException();
        }

    }

    private record ConstructionSpecs {

        public bool MachineFrontFromOutside { get; set; }
        public bool MachineBackFromOutside { get; set; }
        public double FrontBackDrop { get; set; }
        public double DrillingDepthAdjustment { get; set; }
        public bool FullHeightFront { get; set; }
        public bool FullHeightBack { get; set; }

        public static ConstructionSpecs ReadFromSheet(Worksheet worksheet) {
            throw new NotImplementedException();
        }

    }

}
