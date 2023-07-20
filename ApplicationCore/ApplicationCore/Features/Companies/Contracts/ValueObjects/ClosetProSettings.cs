namespace ApplicationCore.Features.Companies.Contracts.ValueObjects;

public record ClosetProSettings {

    public string ToeKickSKU { get; set; } = "TK-F";
    public string AdjustableShelfSKU { get; set; } = "SA5";
    public string FixedShelfSKU { get; set; } = "SF";

}
