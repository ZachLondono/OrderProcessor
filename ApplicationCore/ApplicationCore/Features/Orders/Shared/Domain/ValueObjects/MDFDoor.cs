using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class MDFDoor {

    public int Qty { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public string Material { get; }
    public string FramingBead { get; }
    public string EdgeDetail { get; }
    public DoorFrame FrameSize { get; }
    public Dimension PanelDrop { get; }

    public MDFDoor(int qty, Dimension height, Dimension width, string material, string framingBead, string edgeDetail, DoorFrame frameSize, Dimension panelDrop) {
        Qty = qty;
        Height = height;
        Width = width;
        Material = material;
        FramingBead = framingBead;
        EdgeDetail = edgeDetail;
        FrameSize = frameSize;
        PanelDrop = panelDrop;
    }

}
