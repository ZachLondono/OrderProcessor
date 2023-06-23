using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Products.DoweledDrawerBoxTests;

public class DoweledDrawerBoxConstructionBuilder {

        public IDictionary<Dimension, Dimension[]> DowelPositionsByHeight { get; init; } = new Dictionary<Dimension, Dimension[]>();
        public Dimension DowelDepth { get; init; } = Dimension.FromMillimeters(10);
        public Dimension DowelDiameter { get; init; } = Dimension.FromMillimeters(8);
        public string BottomDadoToolName { get; init; } = "1-4Strt";
        public Dimension BottomDadoToolDiameter { get; init; } = Dimension.FromMillimeters(6.35);
        public Dimension BottomDadoStartHeight { get; init; } = Dimension.FromInches(0.5);
        public Dimension BottomDadoDepth { get; init; } = Dimension.FromMillimeters(8);
        public Dimension BottomDadoHeightOversize { get; init; } = Dimension.FromMillimeters(1.5);
        public Dimension FrontBackBottomDadoLengthOversize { get; init; } = Dimension.FromMillimeters(3);
        public Dimension BottomUndersize { get; init; } = Dimension.FromMillimeters(1);
        public string UMSlidePocketToolName { get; init; } = "Pocket9";
        public Dimension UMSlidePocketDiameter { get; init; } = Dimension.FromMillimeters(9);
        public Dimension UMSlideMaxDistanceOffOutsideFace { get; init; } = Dimension.FromInches(5.0/8.0);
        
        public DoweledDrawerBoxConstruction Build() {
            return new() {
                DowelPositionsByHeight = DowelPositionsByHeight,
                DowelDepth = DowelDepth,
                DowelDiameter = DowelDiameter,
                BottomDadoToolName = BottomDadoToolName,
                BottomDadoToolDiameter = BottomDadoToolDiameter,
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
