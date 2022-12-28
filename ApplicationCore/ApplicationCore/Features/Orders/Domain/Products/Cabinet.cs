using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain.Products;

internal abstract class Cabinet : IProduct {

    public int Qty { get; }
    public decimal UnitPrice { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public Dimension Depth { get; }
    public CabinetMaterial BoxMaterial { get; }
    public CabinetMaterial FinishMaterial { get; }
    public CabinetSide RightSide { get; }
    public CabinetSide LeftSide { get; }

    public Cabinet(int qty, decimal unitPrice,
                Dimension height, Dimension width, Dimension depth,
                CabinetMaterial boxMaterial, CabinetMaterial finishMaterial,
                CabinetSide rightSide, CabinetSide leftSide) {

        Qty = qty;
        UnitPrice = unitPrice;
        Height = height;
        Width = width;
        Depth = depth;
        BoxMaterial = boxMaterial;
        FinishMaterial = finishMaterial;
        RightSide = rightSide;
        LeftSide = leftSide;

        if (boxMaterial.Core == CabinetMaterialCore.Plywood && finishMaterial.Core == CabinetMaterialCore.Flake)
            throw new InvalidOperationException("Cannot create cabinet with plywood box and flake finished side");

    }

}
