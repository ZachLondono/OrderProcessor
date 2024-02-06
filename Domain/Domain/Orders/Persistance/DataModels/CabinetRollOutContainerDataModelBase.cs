using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Persistance.DataModels;

public abstract class CabinetRollOutContainerDataModelBase : CabinetDrawerBoxContainerDataModelBase {

    public Dimension[] ROPositions { get; set; } = Array.Empty<Dimension>();
    public RollOutBlockPosition ROBlockType { get; set; }
    public bool ROScoopFront { get; set; }

    protected RollOutOptions GetRollOutOptions() => new(ROPositions, ROScoopFront, ROBlockType);

}
