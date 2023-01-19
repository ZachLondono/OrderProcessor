using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

internal class DoorFrame {

    public required Dimension TopRail { get; init; }
    public required Dimension BottomRail { get; init; }
    public required Dimension LeftStile { get; init; }
    public required Dimension RightStile { get; init; }

    public DoorFrame(Dimension frameWidth) {
        TopRail = frameWidth;
        BottomRail = frameWidth;
        LeftStile = frameWidth;
        RightStile = frameWidth;
    }

    public DoorFrame(Dimension rails, Dimension stiles) {
        TopRail = rails;
        BottomRail = rails;
        LeftStile = stiles;
        RightStile = rails;
    }

    public DoorFrame() { }


}