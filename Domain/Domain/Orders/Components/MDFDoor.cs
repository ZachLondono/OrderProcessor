using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Components;

public class MDFDoor : MDFDoorOptions, IComponent {

    public int Qty { get; }
    public int ProductNumber { get; }
    public DoorType Type { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public string Note { get; }
    public DoorFrame FrameSize { get; }
    public DoorOrientation Orientation { get; }
    public AdditionalOpening[] AdditionalOpenings { get; }

    public MDFDoor(int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, DoorFrame frameSize, string material, Dimension thickness, string framingBead, string edgeDetail, string panelDetail, Dimension panelDrop, DoorOrientation orientation, AdditionalOpening[] additionalOpenings, string? paintColor)
     : base(material, thickness, framingBead, edgeDetail, panelDetail, panelDrop, paintColor) {
        Qty = qty;
        ProductNumber = productNumber;
        Type = type;
        Height = height;
        Width = width;
        Note = note;
        FrameSize = frameSize;
        Orientation = orientation;
        AdditionalOpenings = additionalOpenings;
    }

    public MDFDoor(int qty, int productNumber, DoorType type, Dimension height, Dimension width, string note, DoorFrame frameSize, DoorOrientation orientation, AdditionalOpening[] additionalOpenings, MDFDoorOptions options)
        : base(options.Material, options.Thickness, options.FramingBead, options.EdgeDetail, options.PanelDetail, options.PanelDrop, options.PaintColor) {
        Qty = qty;
        ProductNumber = productNumber;
        Type = type;
        Height = height;
        Width = width;
        Note = note;
        FrameSize = frameSize;
        Orientation = orientation;
        AdditionalOpenings = additionalOpenings;
    }

}
