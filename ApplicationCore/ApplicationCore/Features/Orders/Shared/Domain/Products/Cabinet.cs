using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public abstract class Cabinet : IProduct {

    public Guid Id { get; }
    public int Qty { get; }
    public decimal UnitPrice { get; }
    public string Room { get; }
    public bool Assembled { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public Dimension Depth { get; }
    public CabinetMaterial BoxMaterial { get; }
    public CabinetMaterial FinishMaterial { get; }
    public string EdgeBandingColor { get; }
    public CabinetSide RightSide { get; }
    public CabinetSide LeftSide { get; }

    public Cabinet(Guid id, int qty, decimal unitPrice, string room, bool assembled,
                Dimension height, Dimension width, Dimension depth,
                CabinetMaterial boxMaterial, CabinetMaterial finishMaterial, string edgeBandingColor,
                CabinetSide rightSide, CabinetSide leftSide) {

        Id = id;
        Qty = qty;
        UnitPrice = unitPrice;
        Room = room;
        Assembled = assembled;
        Height = height;
        Width = width;
        Depth = depth;
        BoxMaterial = boxMaterial;
        FinishMaterial = finishMaterial;
        EdgeBandingColor = edgeBandingColor;
        RightSide = rightSide;
        LeftSide = leftSide;

        if (boxMaterial.Core == CabinetMaterialCore.Plywood && finishMaterial.Core == CabinetMaterialCore.Flake)
            throw new InvalidOperationException("Cannot create cabinet with plywood box and flake finished side");

    }

    protected string GetMaterialType() {

        if (BoxMaterial.Core == CabinetMaterialCore.Plywood) return "Sterling 18_5";
        else if (BoxMaterial.Core == CabinetMaterialCore.Flake && FinishMaterial.Core == CabinetMaterialCore.Flake) return "Crown Paint";
        else if (BoxMaterial.Core == CabinetMaterialCore.Flake && FinishMaterial.Core == CabinetMaterialCore.Plywood) return "Crown Veneer";

        throw new InvalidOperationException("Unexpected material combination");

    }

    protected Dictionary<string, PPMaterial> GetFinishMaterials() {
        string finishMaterial = GetFinishMaterialType(FinishMaterial.Core);
        string boxMaterial = GetFinishMaterialType(BoxMaterial.Core);
        return new Dictionary<string, PPMaterial> {
            { "F_Exp_SemiExp", new PPMaterial(finishMaterial, FinishMaterial.Finish) },
            { "F_Exp_Unseen", new PPMaterial(finishMaterial,FinishMaterial.Finish) },
            { "F_Exposed", new PPMaterial(finishMaterial, FinishMaterial.Finish) },
            { "F_OvenSupport", new PPMaterial("PRE","Veneer") },
            { "F_SemiExp_Unseen", new PPMaterial(boxMaterial, BoxMaterial.Finish) },
            { "F_SemiExposed", new PPMaterial(boxMaterial, BoxMaterial.Finish) }
        };
    }

    private string GetFinishMaterialType(CabinetMaterialCore material) => material switch {
        CabinetMaterialCore.Flake => "Mela",
        CabinetMaterialCore.Plywood => "Veneer",
        _ => "Mela"
    };

    protected Dictionary<string, PPMaterial> GetEBMaterials() {
        string finishEBMaterial = GetEBMaterialType(FinishMaterial.Core);
        string boxEBMaterial = GetEBMaterialType(BoxMaterial.Core);
        return new Dictionary<string, PPMaterial>() {
            {"EB_Case", new PPMaterial(finishEBMaterial,EdgeBandingColor) },
            {"EB_Inside", new PPMaterial(boxEBMaterial, BoxMaterial.Finish) },
            {"EB_ShellExposed", new PPMaterial(finishEBMaterial,EdgeBandingColor) },
            {"EB_WallBottom", new PPMaterial(finishEBMaterial,EdgeBandingColor) }
        };
    }

    private string GetEBMaterialType(CabinetMaterialCore material) => material switch {
        CabinetMaterialCore.Flake => "PVC",
        CabinetMaterialCore.Plywood => "Veneer",
        _ => "PVC"
    };

    protected string GetSideOption(CabinetSideType side) => side switch {
        CabinetSideType.AppliedPanel => "0",
        CabinetSideType.Unfinished => "0",
        CabinetSideType.Finished => "1",
        CabinetSideType.IntegratedPanel => "2",
        _ => "0"
    };


    protected string GetAppliedPanelOption() {
        if (LeftSide.Type == CabinetSideType.AppliedPanel && RightSide.Type != CabinetSideType.AppliedPanel) {
            return "1";
        } else if (LeftSide.Type == CabinetSideType.AppliedPanel && RightSide.Type == CabinetSideType.AppliedPanel) {
            return "2";
        } else if (LeftSide.Type != CabinetSideType.AppliedPanel && RightSide.Type == CabinetSideType.AppliedPanel) {
            return "3";
        } else return "0";
    }

}
