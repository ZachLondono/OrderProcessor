namespace Domain.Companies.ValueObjects;

public record ExportProfile {

    public bool ExportDBOrder { get; set; }
    public bool ExportMDFDoorOrder { get; set; }
    public bool ExportExtFile { get; set; }

}