namespace ApplicationCore.Features.Orders.Domain.ValueObjects;

public record CabinetSide {

    public CabinetSideType Type { get; }
    public MDFDoorOptions? DoorOptions { get; }

    public CabinetSide(CabinetSideType type, MDFDoorOptions? doorOptions = null) {
        if (doorOptions is not null) {

            Type = type;
            DoorOptions = doorOptions;

        } else {

            if (type == CabinetSideType.IntegratedPanel || type == CabinetSideType.AppliedPanel)
                throw new InvalidOperationException("MDFDoorOptions are required when creating a cabinet side with a door");

            Type = type;
            DoorOptions = null;

        }
    }

}
