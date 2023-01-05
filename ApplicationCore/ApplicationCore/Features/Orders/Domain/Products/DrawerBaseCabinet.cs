using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain.Products;

public class DrawerBaseCabinet : Cabinet, IPPProductContainer {

    public MDFDoorOptions? Fronts { get; }
    public VerticalDrawerBank Drawers { get; }

    public static DrawerBaseCabinet Create(int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        VerticalDrawerBank drawers, MDFDoorOptions? fronts) {
        return new(Guid.NewGuid(), qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide, drawers, fronts);
    }

    private DrawerBaseCabinet(Guid id, int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        VerticalDrawerBank drawers, MDFDoorOptions? fronts)
                        : base(id, qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide) {

        if (drawers.FaceHeights.Count() > 5 || !drawers.FaceHeights.Any())
            throw new InvalidOperationException("Invalid number of drawers");

        Drawers = drawers;
        Fronts = fronts;
    }

    public IEnumerable<PPProduct> GetPPProducts() {
        // TODO: add option for no doors
        string doorType = (Fronts is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Room, GetProductName(), "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), new());
    }

    private string GetProductName() {
        return $"DB{Drawers.FaceHeights.Count()}D";
    }

    private Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", GetSideOption(LeftSide.Type) },
            { "FinishedRight", GetSideOption(RightSide.Type) },
            { "AppliedPanel", GetAppliedPanelOption() },
        };

        int index = 1;
        foreach (var height in Drawers.FaceHeights) {
            parameters.Add($"DrawerH{index++}", height.AsMillimeters().ToString());
        }

        return parameters;

    }


}