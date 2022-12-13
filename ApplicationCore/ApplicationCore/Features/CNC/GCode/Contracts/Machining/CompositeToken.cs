namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public abstract record CompositeToken : MachiningOperation {
    public abstract IEnumerable<MachiningOperation> GetComponents();
}