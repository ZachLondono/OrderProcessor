using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal abstract class CabinetDrawerBoxContainerDataModelBase : CabinetDataModelBase {

    public CabinetDrawerBoxMaterial DBMaterial { get; set; }
    public DrawerSlideType DBSlideType { get; set; }

    protected CabinetDrawerBoxOptions GetDrawerBoxOptions() => new(DBMaterial, DBSlideType);

}
