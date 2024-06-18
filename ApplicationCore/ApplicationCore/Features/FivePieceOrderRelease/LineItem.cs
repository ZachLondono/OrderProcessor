using Domain.ValueObjects;

namespace ApplicationCore.Features.FivePieceOrderRelease;

internal record LineItem(int PartNum,
                         string Description,
                         int Line,
                         int Qty,
                         Dimension Width,
                         Dimension Height,
                         string SpecialFeatures,
                         Dimension LeftStile,
                         Dimension RightStile,
                         Dimension TopRail,
                         Dimension BottomRail);
