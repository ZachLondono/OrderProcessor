using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;

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
