using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class MDFDoor {

    public int Qty { get; }
    public int ProductNumber { get; }
    public DoorType Type { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public string Note { get; }
    public string Material { get; }
    public string FramingBead { get; }
    public string EdgeDetail { get; }
    public DoorFrame FrameSize { get; }
    public Dimension PanelDrop { get; }
    public string? PaintColor { get; set; }

    public MDFDoor(int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, string material, string framingBead, string edgeDetail, DoorFrame frameSize, Dimension panelDrop, string? paintColor) {
        Qty = qty;
        ProductNumber = productNumber;
        Type = type;
        Height = height;
        Width = width;
        Note = note;
        Material = material;
        FramingBead = framingBead;
        EdgeDetail = edgeDetail;
        FrameSize = frameSize;
        PanelDrop = panelDrop;
        PaintColor = paintColor;
    }

}
