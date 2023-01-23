using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public class DrawerBaseCabinet : Cabinet, IPPProductContainer {

    public IToeType ToeType { get; }
    public MDFDoorOptions? Fronts { get; }
    public VerticalDrawerBank Drawers { get; }

    public static DrawerBaseCabinet Create(int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        IToeType toeType, VerticalDrawerBank drawers, MDFDoorOptions? fronts) {
        return new(Guid.NewGuid(), qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide, toeType, drawers, fronts);
    }

    private DrawerBaseCabinet(Guid id, int qty, decimal unitPrice, string room, bool assembled,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                        CabinetSide rightSide, CabinetSide leftSide,
                        IToeType toeType, VerticalDrawerBank drawers, MDFDoorOptions? fronts)
                        : base(id, qty, unitPrice, room, assembled, height, width, depth, boxMaterial, finishMaterial, edgeBandingColor, rightSide, leftSide) {

        if (drawers.FaceHeights.Count() > 5)
            throw new InvalidOperationException("Invalid number of drawers");

        Drawers = drawers;
        Fronts = fronts;
        ToeType = toeType;
    }

    public IEnumerable<PPProduct> GetPPProducts() {
        // TODO: add option for no doors
        string doorType = (Fronts is null) ? "Slab" : "Buyout";
        yield return new PPProduct(Room, GetProductName(), "Royal2", GetMaterialType(), doorType, "Standard", GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetOverrideParameters(), new());
    }

    private string GetProductName() {
        if (!Drawers.FaceHeights.Any()) return "DB1D";
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
            if (index == 5) break; // in 5 drawer cabinets, the fifth drawer must be calculated by PSI
            parameters.Add($"DrawerH{index++}", height.AsMillimeters().ToString());
        }

        return parameters;

    }

    private Dictionary<string, string> GetOverrideParameters() {

        var parameters = new Dictionary<string, string>();
        if (ToeType.PSIParameter != "2") {
            parameters.Add("__ToeBaseType", ToeType.PSIParameter);
            if (ToeType.PSIParameter == "3") {
                parameters.Add("__ToeBaseHeight", "0");
            }
        }

        if (Drawers.FaceHeights.Any() && Drawers.SlideType == DrawerSlideType.SideMount) {
            parameters.Add("_DrawerRunType", "4");
        }

        return parameters;

    }


}