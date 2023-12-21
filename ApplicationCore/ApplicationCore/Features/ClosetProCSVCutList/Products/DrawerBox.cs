using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Components;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.ClosetProCSVCutList.Products;

public class DrawerBox : IClosetProProduct {

    public required int Qty { get; init; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int PartNumber { get; init; }

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

        DrawerSlideType slideType = UnderMountNotches ? DrawerSlideType.UnderMount : DrawerSlideType.SideMount;
        string materialName = DovetailDrawerBoxConfig.FINGER_JOINT_BIRCH;
        string bottomMaterial = "1/4\" Ply";
        string clips = "Hettich";
        string accessory = "None";
        string notch = UnderMountNotches ? "Std_Notch" : "No_Notch";

        return factory.CreateDovetailDrawerBoxBuilder()
            .WithOptions(new(materialName, materialName, materialName, bottomMaterial, clips, notch, accessory, LogoPosition.None, scoopFront: ScoopFront))
            .WithDrawerFaceHeight(Height)
            .WithInnerCabinetWidth(Width, 1, slideType)
            .WithInnerCabinetDepth(Depth, slideType, false)
            .WithQty(Qty)
            .WithProductNumber(PartNumber)
            .BuildProduct(UnitPrice, Room);

    }

}
