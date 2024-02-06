using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;

namespace Domain.Orders.Persistance.DataModels;

public abstract class CabinetDrawerBoxContainerDataModelBase : CabinetDataModelBase {

    public CabinetDrawerBoxMaterial DBMaterial { get; set; }
    public DrawerSlideType DBSlideType { get; set; }

    protected CabinetDrawerBoxOptions GetDrawerBoxOptions() => new(DBMaterial, DBSlideType);

}
