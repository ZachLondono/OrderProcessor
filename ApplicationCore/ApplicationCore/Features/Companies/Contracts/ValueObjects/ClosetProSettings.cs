using Domain.ValueObjects;

namespace ApplicationCore.Features.Companies.Contracts.ValueObjects;

public record ClosetProSettings {

    public string ToeKickSKU { get; set; } = "TK-F";

    public string AdjustableShelfSKU { get; set; } = "SA5";
    public string FixedShelfSKU { get; set; } = "SF";

    public string LFixedShelfSKU { get; set; } = "SFL19";
    public string LAdjustableShelfSKU { get; set; } = "SAL19";
    public Dimension LShelfRadius { get; set; } = Dimension.Zero;

    public string DiagonalFixedShelfSKU { get; set; } = "SFD19";
    public string DiagonalAdjustableShelfSKU { get; set; } = "SAD19";

    public string DoweledDrawerBoxMaterialFinish { get; set; } = "White";

    public Dimension VerticalPanelBottomRadius { get; set; } = Dimension.FromMillimeters(51);

    //public string DividerShelfDrillingType { get; set; } = "";
    //public string DividerPanelDrillingType { get; set; } = "";

}
