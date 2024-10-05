using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Tests.Unit.Orders.DoweledDrawerBoxes;

public class DoweledDrawerBoxConstructionBuilder {

    public IDictionary<Dimension, Dimension[]> DowelPositionsByHeight { get; init; } = new Dictionary<Dimension, Dimension[]>();
    public Dimension DowelDepth { get; init; } = Dimension.FromMillimeters(10);
    public Dimension DowelDiameter { get; init; } = Dimension.FromMillimeters(8);
    public string SmallBottomDadoToolName { get; init; } = "1-4Strt";
    public Dimension SmallBottomDadoToolDiameter { get; init; } = Dimension.FromMillimeters(6.35);
    public string LargeBottomDadoToolName { get; init; } = "1-2Dado";
    public Dimension LargeBottomDadoToolDiameter { get; init; } = Dimension.FromMillimeters(12.7);
    public Dimension LargeBottomDadoToolMinimum { get; init; } = Dimension.FromMillimeters(12.7);
    public Dimension BottomDadoScoringDepth { get; init; } = Dimension.FromMillimeters(1.5);
    public Dimension BottomDadoStartHeight { get; init; } = Dimension.FromInches(0.5);
    public Dimension BottomDadoDepth { get; init; } = Dimension.FromMillimeters(8);
    public Dimension BottomDadoHeightOversize { get; init; } = Dimension.FromMillimeters(1.5);
    public Dimension FrontBackBottomDadoLengthOversize { get; init; } = Dimension.FromMillimeters(3);
    public Dimension BottomUndersize { get; init; } = Dimension.FromMillimeters(1);
    public string UMSlidePocketToolName { get; init; } = "Pocket9";
    public Dimension UMSlidePocketDiameter { get; init; } = Dimension.FromMillimeters(9);
    public Dimension UMSlideMaxDistanceOffOutsideFace { get; init; } = Dimension.FromInches(5.0 / 8.0);
    public Dimension WidthUnderSize { get; init; } = Dimension.Zero;

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
