using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal abstract class CabinetRollOutContainerDataModelBase : CabinetDrawerBoxContainerDataModelBase {

    public Dimension[] ROPositions { get; set; } = Array.Empty<Dimension>();
    public RollOutBlockPosition ROBlockType { get; set; }
    public bool ROScoopFront { get; set; }

    protected RollOutOptions GetRollOutOptions() => new(ROPositions, ROScoopFront, ROBlockType);

}
