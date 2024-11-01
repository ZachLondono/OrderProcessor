using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;

namespace Domain.Orders.Persistance.DataModels;

public abstract class CabinetDrawerBoxContainerDataModelBase : CabinetDataModelBase {

    public CabinetDrawerBoxMaterial? DBMaterial { get; set; }
    public DrawerSlideType? DBSlideType { get; set; }

    protected CabinetDrawerBoxOptions? GetDrawerBoxOptions() {

        if (DBMaterial is CabinetDrawerBoxMaterial mat && DBSlideType is DrawerSlideType slideType) {

            return new(mat, slideType);
                
        }

        return null;

    }

}
