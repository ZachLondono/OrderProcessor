namespace ApplicationCore.Features.Companies.Contracts.ValueObjects;

public record ExportProfile {

    public bool ExportDBOrder { get; set; }
    public bool ExportMDFDoorOrder { get; set; }
    public bool ExportExtFile { get; set; }
    public string OutputDirectory { get; set; } = string.Empty;

}