using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Components;

public class MDFDoor {

    public int Qty { get; }
    public int ProductNumber { get; }
    public DoorType Type { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public string Note { get; }
    public DoorFrame FrameSize { get; }
    public DoorOrientation Orientation { get; }
    /// <summary>
    /// Specifies the type of panel for the main opening. Additional openings must set this property separately.
    /// </summary>
    public MDFDoorPanel Panel { get; }
    public AdditionalOpening[] AdditionalOpenings { get; }
    public string Material { get; }
    public Dimension Thickness { get; }
    public string FramingBead { get; }
    public string EdgeDetail { get; }
    public string PanelDetail { get; }
    public Dimension PanelDrop { get; }
    public MDFDoorFinish Finish { get; }

    public MDFDoor(int qty,
				   int productNumber,
				   DoorType type,
				   Dimension height,
				   Dimension width,
				   string note,
				   DoorFrame frameSize,
				   string material,
				   Dimension thickness,
				   string framingBead,
				   string edgeDetail,
				   string panelDetail,
				   Dimension panelDrop,
				   DoorOrientation orientation,
				   AdditionalOpening[] additionalOpenings,
				   MDFDoorFinish finish,
                   MDFDoorPanel panel) {
        Qty = qty;
        ProductNumber = productNumber;
        Type = type;
        Height = height;
        Width = width;
        Note = note;
        FrameSize = frameSize;
        Orientation = orientation;
        Panel = panel;
        AdditionalOpenings = additionalOpenings;
        Material = material;
        Thickness = thickness;
        FramingBead = framingBead;
        EdgeDetail = edgeDetail;
        PanelDetail = panelDetail;
        PanelDrop = panelDrop;
        Finish = finish;
    }

    public MDFDoorOptions GetMDFDoorOptions() => new(Material,
													 Thickness,
													 FramingBead,
													 EdgeDetail,
													 PanelDetail,
													 PanelDrop,
													 Finish);

}
