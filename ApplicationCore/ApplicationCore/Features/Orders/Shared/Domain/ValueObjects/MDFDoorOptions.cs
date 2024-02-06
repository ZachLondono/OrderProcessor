using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class MDFDoorOptions {

    public string Material { get; }
    public Dimension Thickness { get; }
    public string FramingBead { get; }
    public string EdgeDetail { get; }
    public string PanelDetail { get; }
    public Dimension PanelDrop { get; }
    public string? PaintColor { get; }

    public MDFDoorOptions(string material, Dimension thickness, string framingBead, string edgeDetail, string panelDetail, Dimension panelDrop, string? paintColor) {
        Material = material;
        Thickness = thickness;
        FramingBead = framingBead;
        EdgeDetail = edgeDetail;
        PanelDetail = panelDetail;
        PanelDrop = panelDrop;
        PaintColor = paintColor;
    }

}
