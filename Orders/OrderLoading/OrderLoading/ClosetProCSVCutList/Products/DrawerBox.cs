using Domain.Companies.ValueObjects;
using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.ValueObjects;

namespace OrderLoading.ClosetProCSVCutList.Products;

public class DrawerBox : IClosetProProduct {

    public required int Qty { get; set; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int PartNumber { get; init; }

    /// <summary>
    /// The face height of the drawer box / The opening height of the space where the drawer box is going
    /// </summary>
    public required Dimension Height { get; init; }
    public required Dimension Width { get; init; }
    public required Dimension Depth { get; init; }
    public required bool ScoopFront { get; init; }
    public required bool UnderMountNotches { get; init; }
    public required DrawerBoxType Type { get; init; }

    public IProduct ToProduct(ComponentBuilderFactory factory, ClosetProSettings settings) => Type switch {
        DrawerBoxType.Dovetail => ToDovetailDrawerBox(factory),
        DrawerBoxType.Dowel => ToDowelDrawerBox(settings),
        _ => throw new InvalidOperationException("Unexpected drawer box type")
    };

    public DoweledDrawerBoxProduct ToDowelDrawerBox(ClosetProSettings settings) {

        var matThickness = Dimension.FromInches(0.625);
        var material = new DoweledDrawerBoxMaterial(settings.DoweledDrawerBoxMaterialFinish, matThickness, true);
        var botMatThickness = Dimension.FromInches(0.5);
        var botMaterial = new DoweledDrawerBoxMaterial(settings.DoweledDrawerBoxMaterialFinish, botMatThickness, true);

        var heightAdj = Dimension.FromMillimeters(1); // TODO: get this from settings

        // TODO: box height should not be the same as Height property when drawer has drawer front

        return new DoweledDrawerBoxProduct(Guid.NewGuid(),
                                           UnitPrice,
                                           Qty,
                                           Room,
                                           PartNumber,
                                           Height,
                                           Width,
                                           Depth,
                                           material,
                                           material,
                                           material,
                                           botMaterial,
                                           UnderMountNotches,
                                           heightAdj);

    }

    public DovetailDrawerBoxProduct ToDovetailDrawerBox(ComponentBuilderFactory factory) {

        string materialName = DovetailDrawerBoxConfig.FINGER_JOINT_BIRCH;
        string bottomMaterial = "1/4\" Ply";

        string clips = "None";
        string notch = "No_Notch";
        string accessory = "None";
        DrawerSlideType slideType = DrawerSlideType.SideMount;
        if (UnderMountNotches) {
            notch = "Std_Notch";
            accessory = "Hettich Quadro V6 Slides";
            clips = "Hettich";
            slideType = DrawerSlideType.UnderMount;
        }

        // TODO: Add slides property to drawer box and set the slide type there instead of in the accessory property
        return factory.CreateDovetailDrawerBoxBuilder()
            .WithOptions(new(materialName, materialName, materialName, bottomMaterial, clips, notch, accessory, LogoPosition.None, scoopFront: ScoopFront))
            .WithBoxHeight(GetBoxHeight())
            .WithInnerCabinetWidth(Width, 1, slideType)
            .WithInnerClosetBayDepth(Depth, slideType)
            .WithQty(Qty)
            .WithProductNumber(PartNumber)
            .BuildProduct(UnitPrice, Room);

    }

    private readonly static Dimension _verticalClearance = Dimension.FromMillimeters(41);
    private readonly static List<(Dimension, Dimension)> _stdHeights = [
        ( Dimension.FromInches(6.25), Dimension.FromInches(2.5)),
        ( Dimension.FromInches(7.5), Dimension.FromInches(4.125)),
        ( Dimension.FromInches(10), Dimension.FromInches(6.0)),
        ( Dimension.FromInches(13.75), Dimension.FromInches(8.25)),
        ( Dimension.FromInches(9999), Dimension.FromInches(12))
    ];

    public Dimension GetBoxHeight() {

        if (ScoopFront) return Height;

        foreach (var (maxOpeningHeight, boxHeight) in _stdHeights) {

            if (Height <= maxOpeningHeight) {
                return boxHeight;
            }

        }

        return Dimension.Zero;

    }

}
