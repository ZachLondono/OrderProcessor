using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class MDFDoor : MDFDoorOptions {

    public int Qty { get; }
    public int ProductNumber { get; }
    public DoorType Type { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public string Note { get; }
    public DoorFrame FrameSize { get; }

    public MDFDoor(int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, DoorFrame frameSize, string material, Dimension thickness, string framingBead, string edgeDetail, string panelDetail, Dimension panelDrop, string? paintColor)
     : base(material, thickness, framingBead, edgeDetail, panelDetail, panelDrop, paintColor) {
        Qty = qty;
        ProductNumber = productNumber;
        Type = type;
        Height = height;
        Width = width;
        Note = note;
        FrameSize = frameSize;
    }

    public MDFDoor(int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, DoorFrame frameSize, MDFDoorOptions options)
        : base(options.Material, options.Thickness, options.FramingBead, options.EdgeDetail, options.PanelDetail, options.PanelDrop, options.PaintColor) {
        Qty = qty;
        ProductNumber = productNumber;
        Type = type;
        Height = height;
        Width = width;
        Note = note;
        FrameSize = frameSize;
    }

}
