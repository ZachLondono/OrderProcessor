using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

public abstract class CabinetBuilder<TCabinet> where TCabinet : Cabinet {

    public int Qty { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Room { get; private set; }
    public bool Assembled { get; private set; }
    public Dimension Width { get; private set; }
    public Dimension Height { get; private set; }
    public Dimension Depth { get; private set; }
    public CabinetMaterial BoxMaterial { get; private set; }
    public CabinetFinishMaterial FinishMaterial { get; private set; }
    public CabinetSlabDoorMaterial? SlabDoorMaterial { get; private set; }
    public MDFDoorOptions? MDFDoorOptions { get; private set; }
    public string EdgeBandingColor { get; private set; }
    public CabinetSideType LeftSideType { get; set; }
    public CabinetSideType RightSideType { get; set; }
    public int ProductNumber { get; private set; }
    public string Comment { get; private set; }

    public CabinetBuilder() {
        Qty = 0;
        Room = string.Empty;
        UnitPrice = 0;
        Assembled = false;
        Width = Dimension.Zero;
        Height = Dimension.Zero;
        Depth = Dimension.Zero;
        BoxMaterial = new(string.Empty, CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard);
        FinishMaterial = new(string.Empty, CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard, null);
        SlabDoorMaterial = new(string.Empty, CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard, null);
        MDFDoorOptions = null;
        EdgeBandingColor = string.Empty;
        LeftSideType = CabinetSideType.Unfinished;
        RightSideType = CabinetSideType.Unfinished;
        Comment = string.Empty;
    }

    public CabinetBuilder<TCabinet> WithQty(int qty) {
        Qty = qty;
        return this;
    }

    public CabinetBuilder<TCabinet> WithUnitPrice(decimal unitPrice) {
        UnitPrice = unitPrice;
        return this;
    }

    public CabinetBuilder<TCabinet> WithRoom(string room) {
        Room = room;
        return this;
    }

    public CabinetBuilder<TCabinet> WithAssembled(bool assembled) {
        Assembled = assembled;
        return this;
    }

    public CabinetBuilder<TCabinet> WithWidth(Dimension width) {
        Width = width;
        return this;
    }

    public CabinetBuilder<TCabinet> WithHeight(Dimension height) {
        Height = height;
        return this;
    }

    public CabinetBuilder<TCabinet> WithDepth(Dimension depth) {
        Depth = depth;
        return this;
    }

    public CabinetBuilder<TCabinet> WithBoxMaterial(CabinetMaterial boxMaterial) {
        BoxMaterial = boxMaterial;
        return this;
    }

    public CabinetBuilder<TCabinet> WithFinishMaterial(CabinetFinishMaterial finishMaterial) {
        FinishMaterial = finishMaterial;
        return this;
    }

    public CabinetBuilder<TCabinet> WithSlabDoorMaterial(CabinetSlabDoorMaterial? slabDoorMaterial) {
        SlabDoorMaterial = slabDoorMaterial;
        return this;
    }

    public CabinetBuilder<TCabinet> WithMDFDoorOptions(MDFDoorOptions? mdfDoorOptions) {
        MDFDoorOptions = mdfDoorOptions;
        return this;
    }

    public CabinetBuilder<TCabinet> WithEdgeBandingColor(string edgeBandingColor) {
        EdgeBandingColor = edgeBandingColor;
        return this;
    }

    public CabinetBuilder<TCabinet> WithLeftSideType(CabinetSideType leftSideType) {
        LeftSideType = leftSideType;
        return this;
    }

    public CabinetBuilder<TCabinet> WithRightSideType(CabinetSideType rightSideType) {
        RightSideType = rightSideType;
        return this;
    }

    public CabinetBuilder<TCabinet> WithProductNumber(int productNumber) {
        ProductNumber = productNumber;
        return this;
    }

    public CabinetBuilder<TCabinet> WithComment(string comment) {
        Comment = comment;
        return this;
    }

    public abstract TCabinet Build();

}