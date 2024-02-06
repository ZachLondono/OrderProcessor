using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products.Cabinets;

public abstract class GarageCabinet : Cabinet {

    public bool IsGarage { get; set; }

    protected GarageCabinet(Guid id, int qty, decimal unitPrice, int productNumber, string room, bool assembled, Dimension height, Dimension width, Dimension depth, CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial, CabinetSlabDoorMaterial? slabDoorMaterial, MDFDoorOptions? mdfDoorOptions, string edgeBandingColor, CabinetSideType rightSideType, CabinetSideType leftSideType, string comment)
        : base(id, qty, unitPrice, productNumber, room, assembled, height, width, depth, boxMaterial, finishMaterial, slabDoorMaterial, mdfDoorOptions, edgeBandingColor, rightSideType, leftSideType, comment) {
    }

    protected override string GetMaterialType() {
        if (IsGarage) return "Garage";
        return base.GetMaterialType();
    }

}
