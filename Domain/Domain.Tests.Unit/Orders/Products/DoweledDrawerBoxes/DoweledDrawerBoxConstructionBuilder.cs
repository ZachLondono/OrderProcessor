using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Tests.Unit.Orders.Products.DoweledDrawerBoxes;

public class DoweledDrawerBoxConstructionBuilder {

    public IDictionary<Dimension, Dimension[]> DowelPositionsByHeight { get; init; } = new Dictionary<Dimension, Dimension[]>();
    public Dimension DowelDepth { get; set; } = Dimension.FromMillimeters(10);
    public Dimension DowelDiameter { get; set; } = Dimension.FromMillimeters(8);
    public string SmallBottomDadoToolName { get; set; } = "1-4Strt";
    public Dimension SmallBottomDadoToolDiameter { get; set; } = Dimension.FromMillimeters(6.35);
    public string LargeBottomDadoToolName { get; set; } = "1-2Dado";
    public Dimension LargeBottomDadoToolDiameter { get; set; } = Dimension.FromMillimeters(12.7);
    public Dimension LargeBottomDadoToolMinimum { get; set; } = Dimension.FromMillimeters(12.7);
    public Dimension BottomDadoScoringDepth { get; set; } = Dimension.FromMillimeters(1.5);
    public Dimension BottomDadoStartHeight { get; set; } = Dimension.FromInches(0.5);
    public Dimension BottomDadoDepth { get; set; } = Dimension.FromMillimeters(8);
    public Dimension BottomDadoHeightOversize { get; set; } = Dimension.FromMillimeters(1.5);
    public Dimension FrontBackBottomDadoLengthOversize { get; set; } = Dimension.FromMillimeters(3);
    public Dimension BottomUndersize { get; set; } = Dimension.FromMillimeters(1);
    public string UMSlidePocketToolName { get; set; } = "Pocket9";
    public Dimension UMSlidePocketDiameter { get; set; } = Dimension.FromMillimeters(9);
    public Dimension UMSlideMaxDistanceOffOutsideFace { get; set; } = Dimension.FromInches(5.0 / 8.0);
    public Dimension WidthUnderSize { get; set; } = Dimension.Zero;

    public DoweledDrawerBoxConstruction Build() {
        return new() {
            WidthUndersize = WidthUnderSize,
            DowelPositionsByHeight = DowelPositionsByHeight,
            DowelDepth = DowelDepth,
            DowelDiameter = DowelDiameter,
            SmallBottomDadoToolName = SmallBottomDadoToolName,
            SmallBottomDadoToolDiameter = SmallBottomDadoToolDiameter,
            LargeBottomDadoToolDiameter = LargeBottomDadoToolDiameter,
            LargeBottomDadoToolName = LargeBottomDadoToolName,
            LargeBottomDadoToolMinimum = LargeBottomDadoToolMinimum,
            BottomDadoScoringDepth = BottomDadoScoringDepth,
            BottomDadoStartHeight = BottomDadoStartHeight,
            BottomDadoDepth = BottomDadoDepth,
            BottomDadoHeightOversize = BottomDadoHeightOversize,
            FrontBackBottomDadoLengthOversize = FrontBackBottomDadoLengthOversize,
            BottomUndersize = BottomUndersize,
            UMSlidePocketToolName = UMSlidePocketToolName,
            UMSlidePocketDiameter = UMSlidePocketDiameter,
            UMSlideMaxDistanceOffOutsideFace = UMSlideMaxDistanceOffOutsideFace
        };
    }

}
