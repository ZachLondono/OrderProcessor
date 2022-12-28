namespace ApplicationCore.Features.Orders.Domain.ValueObjects;

internal record CabinetSide {

    public CabinetSideType Type { get; }
    public MDFDoorOptions? DoorOptions { get; }

    public CabinetSide(CabinetSideType type) {

        if (type == CabinetSideType.IntegratedPanel || type == CabinetSideType.AppliedPanel)
            throw new InvalidOperationException("MDFDoorOptions are required when creating a cabinet side with a door");

        Type = type;
        DoorOptions = null;

    }

    public CabinetSide(CabinetSideType type, MDFDoorOptions doorOptions) {
        Type = type;
        DoorOptions = doorOptions;
    }

}
