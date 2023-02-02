namespace ApplicationCore.Features.CNC.ReleasePDF.Contracts;

public class ReleasedJob {

    public string JobName { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string VendorName { get; init; } = string.Empty;
    public DateTime OrderDate { get; init; }
    public DateTime ReleaseDate { get; init; }
    public IEnumerable<MachineRelease> Releases { get; init; } = Enumerable.Empty<MachineRelease>();

}
