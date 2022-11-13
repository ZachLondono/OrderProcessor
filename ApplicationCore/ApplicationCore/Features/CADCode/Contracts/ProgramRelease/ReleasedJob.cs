namespace ApplicationCore.Features.CADCode.Contracts.ProgramRelease;

public class ReleasedJob
{

    public string JobName { get; init; } = string.Empty;
    public IEnumerable<MachineRelease> Releases { get; init; } = Enumerable.Empty<MachineRelease>();

}
