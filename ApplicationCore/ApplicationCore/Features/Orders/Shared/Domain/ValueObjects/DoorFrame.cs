using ApplicationCore.Shared.Domain;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class DoorFrame {

    public required Dimension TopRail { get; init; }
    public required Dimension BottomRail { get; init; }
    public required Dimension LeftStile { get; init; }
    public required Dimension RightStile { get; init; }

    [SetsRequiredMembers]
    public DoorFrame(Dimension frameWidth) {
        TopRail = frameWidth;
        BottomRail = frameWidth;
        LeftStile = frameWidth;
        RightStile = frameWidth;
    }

    [SetsRequiredMembers]
    public DoorFrame(Dimension rails, Dimension stiles) {
        TopRail = rails;
        BottomRail = rails;
        LeftStile = stiles;
        RightStile = stiles;
    }

    public DoorFrame() { }

}