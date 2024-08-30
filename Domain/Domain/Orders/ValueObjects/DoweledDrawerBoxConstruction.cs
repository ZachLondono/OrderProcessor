using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record DoweledDrawerBoxConstruction {

    /// <summary>
    /// The key is the maximum height for which the dowel positions (value) are valid
    /// </summary>
    public required IDictionary<Dimension, Dimension[]> DowelPositionsByHeight { get; init; }
    public required Dimension DowelDepth { get; init; }
    public required Dimension DowelDiameter { get; init; }

    public required string SmallBottomDadoToolName { get; init; }
    public required Dimension SmallBottomDadoToolDiameter { get; init; }

    public required string LargeBottomDadoToolName { get; init; }
    public required Dimension LargeBottomDadoToolDiameter { get; init; }

    public required Dimension LargeBottomDadoToolMinimum { get; init; }
    public required Dimension BottomDadoScoringDepth { get; init; }

    /// <summary>
    /// Distance from bottom of drawer box to bottom of the dado for the bottom
    /// </summary>
    public required Dimension BottomDadoStartHeight { get; init; }
    public required Dimension BottomDadoDepth { get; init; }
    public required Dimension BottomDadoHeightOversize { get; init; }
    public required Dimension FrontBackBottomDadoLengthOversize { get; init; }
    public required Dimension BottomUndersize { get; init; }

    public required string UMSlidePocketToolName { get; init; }
    public required Dimension UMSlidePocketDiameter { get; init; }
    public required Dimension UMSlideMaxDistanceOffOutsideFace { get; init; } // Max distance from outside face of side to drawer slide and outside face of front to clips


    /// <summary>
    /// Amount to undersize the width of the drawer box, to account for gaps where the front/back is attached to the sides during construction.
    /// </summary>
    public required Dimension WidthUndersize { get; init; }

    /*
     * These options require the front/back to be machined from the other side or require 2sided machining 
    public required Dimension UMSlideHookHoleCenterFromBottom { get; init; }
    public required Dimension UMSlideHookHoleStep1Diameter { get; init; }
    public required Dimension UMSlideHookHoleStep1Depth { get; init; }
    public required Dimension UMSlideHookHoleStep2Diameter { get; init; }
    public required Dimension UMSlideHookHoleStep2Depth { get; init; }

    public required Dimension DrawerFaceHoleSpread { get; init; } // Maybe switch to hole position off sides instead
    public required Dimension DrawerFaceHoleDiameter { get; init; }
    */

}