using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain.Products;

public abstract class Cabinet : IProduct, IProductPlannerProduct {

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

    public abstract string GetProductName();

    public abstract Dictionary<string, string> GetParameters();

	public abstract Dictionary<string, string> GetOverrideParameters();

}
