using Domain.Orders.Enums;
using Domain.Orders.Exceptions;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using Domain.ProductPlanner;

namespace Domain.Orders.Entities.Products.Cabinets;

public abstract class Cabinet : IProduct, IPPProductContainer {

    public Guid Id { get; }
    public int Qty { get; }
    public decimal UnitPrice { get; }
    public int ProductNumber { get; }
    public string Room { get; set; }
    public bool Assembled { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public Dimension Depth { get; }
    public CabinetMaterial BoxMaterial { get; }
    public CabinetFinishMaterial FinishMaterial { get; }
    public CabinetDoorConfiguration DoorConfiguration { get; }
    public string EdgeBandingColor { get; }
    public CabinetSideType RightSideType { get; }
    public CabinetSideType LeftSideType { get; }
    public string Comment { get; }
    public List<string> ProductionNotes { get; set; } = new();
    public abstract string GetDescription();
    public abstract string GetSimpleDescription();

    public CabinetConstruction Construction { get; set; }

    public Dimension InnerWidth => Width - Construction.SideThickness * 2;
    public Dimension InnerDepth => Depth - (Construction.BackThickness + Construction.BackInset);

    public Cabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled,
                Dimension height, Dimension width, Dimension depth,
                CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetDoorConfiguration doorConfiguration, string edgeBandingColor,
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
        DoorConfiguration = doorConfiguration;
        EdgeBandingColor = edgeBandingColor;
        RightSideType = rightSideType;
        LeftSideType = leftSideType;
        Comment = comment;

        //if (boxMaterial.Core == CabinetMaterialCore.Plywood && finishMaterial.Core == CabinetMaterialCore.ParticleBoard)
        //    throw new InvalidProductOptionsException("Cannot create cabinet with plywood box and particle board finished side");

        if (DoorConfiguration.IsMDF && (LeftSideType == CabinetSideType.IntegratedPanel || LeftSideType == CabinetSideType.AppliedPanel || RightSideType == CabinetSideType.IntegratedPanel || RightSideType == CabinetSideType.AppliedPanel))
            throw new InvalidProductOptionsException("MDFDoorOptions are required when creating a cabinet side with a door");

        Construction = boxMaterial.Core switch {
            CabinetMaterialCore.ParticleBoard => new() {
                TopThickness = Dimension.FromMillimeters(19),
                BottomThickness = Dimension.FromMillimeters(19),
                SideThickness = Dimension.FromMillimeters(19),  // Cabinets with plywood finished sides (Crown Veneer) are still treated as having 19mm thick sides
                BackThickness = Dimension.FromMillimeters(13),
                BackInset = Dimension.FromMillimeters(9)
            },
            CabinetMaterialCore.Plywood => new() {
                TopThickness = Dimension.FromMillimeters(17.6),
                BottomThickness = Dimension.FromMillimeters(17.6),
                SideThickness = Dimension.FromMillimeters(17.6),
                BackThickness = Dimension.FromMillimeters(13),
                BackInset = Dimension.FromMillimeters(9)
            },
            _ => throw new InvalidProductOptionsException($"Unexpected cabinet material core type '{boxMaterial.Core}'"),
        };

    }

    public abstract IEnumerable<string> GetNotes();

    protected virtual string GetMaterialType() {

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

    protected virtual Dictionary<string, PPMaterial> GetFinishMaterials() {

        string finishMaterial = GetFinishMaterialType(FinishMaterial.Core);
        string boxMaterial = GetFinishMaterialType(BoxMaterial.Core);
        var materials = new Dictionary<string, PPMaterial> {
            ["F_Exp_SemiExp"] = new PPMaterial(finishMaterial, FinishMaterial.Finish),
            ["F_Exp_Unseen"] = new PPMaterial(finishMaterial, FinishMaterial.Finish),
            ["F_Exposed"] = new PPMaterial(finishMaterial, FinishMaterial.Finish),
            ["F_OvenSupport"] = new PPMaterial("Veneer", "PRE"),
            ["F_SemiExp_Unseen"] = new PPMaterial(boxMaterial, BoxMaterial.Finish),
            ["F_SemiExposed"] = new PPMaterial(boxMaterial, BoxMaterial.Finish)
        };

        DoorConfiguration.Switch(
            slab => {

				string doorMaterial = GetFinishMaterialType(slab.Core);
				materials.Add("F_Door", new PPMaterial(doorMaterial, slab.Finish));
				materials.Add("F_DoorBack", new PPMaterial(doorMaterial, slab.Finish));

            },
            mdf => { },
            byothers => { }
        );

        return materials;

    }

    /// <summary>
    /// Returns the ProductPlanner finish "Material" value for a given material core
    /// </summary>
    public static string GetFinishMaterialType(CabinetMaterialCore core) => core switch {
        CabinetMaterialCore.ParticleBoard => "Mela",
        CabinetMaterialCore.Plywood => "Veneer",
        _ => "Mela"
    };

    protected virtual Dictionary<string, PPMaterial> GetEBMaterials() {
        string finishEBMaterial = GetEBMaterialType(FinishMaterial.Core);
        string boxEBMaterial = GetEBMaterialType(BoxMaterial.Core);
        return new Dictionary<string, PPMaterial>() {
            ["EB_Case"] = new PPMaterial(finishEBMaterial, EdgeBandingColor),
            ["EB_Inside"] = new PPMaterial(boxEBMaterial, BoxMaterial.Finish),
            ["EB_ShellExposed"] = new PPMaterial(finishEBMaterial, EdgeBandingColor),
            ["EB_WallBottom"] = new PPMaterial(finishEBMaterial, EdgeBandingColor),
        };
    }

    public static string GetEBMaterialType(CabinetMaterialCore core) => core switch {
        CabinetMaterialCore.ParticleBoard => "PVC",
        CabinetMaterialCore.Plywood => "Veneer",
        _ => "PVC"
    };

    public const string SLAB_DOOR_TYPE = "Slab";
    public const string BUYOUT_DOOR_TYPE = "Buyout";

    /// <summary>
    /// Returns the ProductPlanner value for "Door/Drawer Front" Style. 
    /// 'Buyout' style means that the doors will not be cut listed buy ProductPlanner. This is for applications where the doors are being ordered from another vendor or the doors are being manufactured using some other method outside of ProductPlanner (MDF doors).
    /// </summary>
    protected string GetDoorType() => DoorConfiguration.IsSlab ? SLAB_DOOR_TYPE : BUYOUT_DOOR_TYPE;

    protected static string GetSideOption(CabinetSideType side) => side switch {
        CabinetSideType.AppliedPanel => "0",
        CabinetSideType.Unfinished => "0",
        CabinetSideType.Finished => "1",
        CabinetSideType.IntegratedPanel => "2",
        CabinetSideType.ConfirmatFinished => "3",
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

    public bool ContainsPPProducts() => true;

    public IEnumerable<PPProduct> GetPPProducts() {
        yield return new PPProduct(Id, Qty, Room, GetProductSku(), ProductNumber, "Royal2", GetMaterialType(), GetDoorType(), "Standard", Comment, GetFinishMaterials(), GetEBMaterials(), GetParameters(), GetParameterOverrides(), GetManualOverrideParameters());
    }

    public abstract string GetProductSku();
    protected virtual IDictionary<string, string> GetParameters() => new Dictionary<string, string>();
    protected virtual IDictionary<string, string> GetParameterOverrides() => new Dictionary<string, string>();
    protected virtual IDictionary<string, string> GetManualOverrideParameters() => new Dictionary<string, string>();

}
