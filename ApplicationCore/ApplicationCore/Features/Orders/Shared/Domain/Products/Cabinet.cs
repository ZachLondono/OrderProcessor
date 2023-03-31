using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public abstract class Cabinet : IProduct {

    public Guid Id { get; }
    public int Qty { get; }
    public decimal UnitPrice { get; }
    public int ProductNumber { get; }
    public string Room { get; }
    public bool Assembled { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public Dimension Depth { get; }
    public CabinetMaterial BoxMaterial { get; }
    public CabinetFinishMaterial FinishMaterial { get; }
    public MDFDoorOptions? MDFDoorOptions { get; init; }
    public string EdgeBandingColor { get; }
    public CabinetSideType RightSideType { get; }
    public CabinetSideType LeftSideType { get; }
    public string Comment { get; }
    public abstract string GetDescription();

    public static CabinetConstruction Construction { get; set; } = new() {
        TopThickness = Dimension.FromMillimeters(19),
        BottomThickness = Dimension.FromMillimeters(19),
        SideThickness = Dimension.FromMillimeters(19),
        BackThickness = Dimension.FromMillimeters(13),
        BackInset = Dimension.FromMillimeters(9)
    };

    public Dimension InnerWidth => Width - Construction.SideThickness * 2;
    public Dimension InnerDepth => Depth - (Construction.BackThickness + Construction.BackInset);

    public Cabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                Dimension height, Dimension width, Dimension depth,
                CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor,
                CabinetSideType rightSideType, CabinetSideType leftSideType, string comment) {

        Id = id;
        Qty = qty;
        UnitPrice = unitPrice;
        ProductNumber = productNumber;
        Room = room;
        Assembled = assembled;
        Height = height;
        Width = width;
        Depth = depth;
        BoxMaterial = boxMaterial;
        FinishMaterial = finishMaterial;
        MDFDoorOptions = mdfDoorOptions;
        EdgeBandingColor = edgeBandingColor;
        RightSideType = rightSideType;
        LeftSideType = leftSideType;
        Comment = comment;

        if (boxMaterial.Core == CabinetMaterialCore.Plywood && finishMaterial.Core == CabinetMaterialCore.ParticleBoard)
            throw new InvalidOperationException("Cannot create cabinet with plywood box and particle board finished side");

        if (LeftSideType == CabinetSideType.IntegratedPanel || LeftSideType == CabinetSideType.AppliedPanel || RightSideType == CabinetSideType.IntegratedPanel || RightSideType == CabinetSideType.AppliedPanel)
            throw new InvalidOperationException("MDFDoorOptions are required when creating a cabinet side with a door");

    }

    public abstract IEnumerable<Supply> GetSupplies();

    protected string GetMaterialType() {

        if (BoxMaterial.Core == CabinetMaterialCore.Plywood) {
            return "Sterling 18_5";
        } else if (BoxMaterial.Core == CabinetMaterialCore.ParticleBoard && FinishMaterial.Core == CabinetMaterialCore.ParticleBoard) {
            if (BoxMaterial.Finish.ToLower() == "white") {
                return "Monarch Core";
            }
            return "Crown Paint";
        } else if (BoxMaterial.Core == CabinetMaterialCore.ParticleBoard && FinishMaterial.Core == CabinetMaterialCore.Plywood) {
            return "Crown Veneer";
        }

        throw new InvalidOperationException("Unexpected material combination");

    }

    protected Dictionary<string, PPMaterial> GetFinishMaterials() {
        string finishMaterial = GetFinishMaterialType(FinishMaterial.Core);
        string boxMaterial = GetFinishMaterialType(BoxMaterial.Core);
        return new Dictionary<string, PPMaterial> {
            ["F_Door"] = new PPMaterial(finishMaterial, FinishMaterial.Finish),
            ["F_DoorBack"] = new PPMaterial(finishMaterial, FinishMaterial.Finish),
            ["F_Exp_SemiExp"] = new PPMaterial(finishMaterial, FinishMaterial.Finish),
            ["F_Exp_Unseen"] = new PPMaterial(finishMaterial, FinishMaterial.Finish),
            ["F_Exposed"] = new PPMaterial(finishMaterial, FinishMaterial.Finish),
            ["F_OvenSupport"] = new PPMaterial("Veneer", "PRE"),
            ["F_SemiExp_Unseen"] = new PPMaterial(boxMaterial, BoxMaterial.Finish),
            ["F_SemiExposed"] = new PPMaterial(boxMaterial, BoxMaterial.Finish)
        };
    }

    private static string GetFinishMaterialType(CabinetMaterialCore material) => material switch {
        CabinetMaterialCore.ParticleBoard => "Mela",
        CabinetMaterialCore.Plywood => "Veneer",
        _ => "Mela"
    };

    protected Dictionary<string, PPMaterial> GetEBMaterials() {
        string finishEBMaterial = GetEBMaterialType(FinishMaterial.Core);
        string boxEBMaterial = GetEBMaterialType(BoxMaterial.Core);
        return new Dictionary<string, PPMaterial>() {
            ["EB_Case"] = new PPMaterial(finishEBMaterial, EdgeBandingColor),
            ["EB_Inside"] = new PPMaterial(boxEBMaterial, BoxMaterial.Finish),
            ["EB_ShellExposed"] = new PPMaterial(finishEBMaterial, EdgeBandingColor),
            ["EB_WallBottom"] = new PPMaterial(finishEBMaterial, EdgeBandingColor),
        };
    }

    private static string GetEBMaterialType(CabinetMaterialCore material) => material switch {
        CabinetMaterialCore.ParticleBoard => "PVC",
        CabinetMaterialCore.Plywood => "Veneer",
        _ => "PVC"
    };

    protected static string GetSideOption(CabinetSideType side) => side switch {
        CabinetSideType.AppliedPanel => "0",
        CabinetSideType.Unfinished => "0",
        CabinetSideType.Finished => "1",
        CabinetSideType.IntegratedPanel => "2",
        _ => "0"
    };

    protected string GetAppliedPanelOption() {
        if (LeftSideType == CabinetSideType.AppliedPanel && RightSideType != CabinetSideType.AppliedPanel) {
            return "1";
        } else if (LeftSideType == CabinetSideType.AppliedPanel && RightSideType == CabinetSideType.AppliedPanel) {
            return "2";
        } else if (LeftSideType != CabinetSideType.AppliedPanel && RightSideType == CabinetSideType.AppliedPanel) {
            return "3";
        } else return "0";
    }

}
