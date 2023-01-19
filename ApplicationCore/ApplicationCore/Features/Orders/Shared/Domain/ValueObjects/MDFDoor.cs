using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

internal class MDFDoor {

    public Dimension Height { get; }
    public Dimension Width { get; }
    public string Material { get; }
    public string FramingBead { get; }
    public string EdgeDetail { get; }
    public DoorFrame FrameSize { get; }
    public Dimension PanelDrop { get; }

    public MDFDoor(Dimension height, Dimension width, string material, string framingBead, string edgeDetail, DoorFrame frameSize, Dimension panelDrop) {
        Height = height;
        Width = width;
        Material = material;
        FramingBead = framingBead;
        EdgeDetail = edgeDetail;
        FrameSize = frameSize;
        PanelDrop = panelDrop;
    }

}
