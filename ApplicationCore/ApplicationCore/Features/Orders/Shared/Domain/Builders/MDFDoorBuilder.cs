using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

public class MDFDoorBuilder {

    private int _qty;
    private int _productNumber;
    private DoorType _type;
    private string _note;
    private string _material;
    private Dimension _thickness;
    private string _framingBead;
    private string _panelDetail;
    private string _edgeDetail;
    private DoorFrame _frameSize;
    private string? _paintColor;

    public MDFDoorBuilder(MDFDoorConfiguration configuration) {

        _type = DoorType.Door;
        _note = string.Empty;
        _material = configuration.Material;
        _thickness = configuration.Thickness;
        _framingBead = configuration.FramingBead;
        _edgeDetail = configuration.EdgeDetail;
        _panelDetail = configuration.PanelDetail;
        _frameSize = new() {
            TopRail = configuration.TopRail,
            BottomRail = configuration.BottomRail,
            LeftStile = configuration.LeftStile,
            RightStile = configuration.RightStile,
        };

    }

    public MDFDoorBuilder WithQty(int qty) {
        _qty = qty;
        return this;
    }

    public MDFDoorBuilder WithMaterial(string material) {
        _material = material;
        return this;
    }

    public MDFDoorBuilder WithThickness(Dimension thickness) {
        _thickness = thickness;
        return this;
    }

    public MDFDoorBuilder WithFramingBead(string framingBead) {
        _framingBead = framingBead;
        return this;
    }

    public MDFDoorBuilder WithEdgeDetail(string edgeDetail) {
        _edgeDetail = edgeDetail;
        return this;
    }

    public MDFDoorBuilder WithPanelDetail(string panelDetail) {
        _panelDetail = panelDetail;
        return this;
    }

    public MDFDoorBuilder WithFrameSize(DoorFrame frameSize) {
        _frameSize = frameSize;
        return this;
    }

    public MDFDoorBuilder WithType(DoorType type) {
        _type = type;
        return this;
    }

    public MDFDoorBuilder WithNote(string note) {
        _note = note;
        return this;
    }

    public MDFDoorBuilder WithProductNumber(int productNumber) {
        _productNumber = productNumber;
        return this;
    }

    public MDFDoorBuilder WithPaintColor(string? paintColor) {
        _paintColor = paintColor;
        return this;
    }

    public MDFDoor Build(Dimension height, Dimension width) {

        return new MDFDoor(_qty, _productNumber, _type, height, width, _note, _frameSize, _material, _thickness, _framingBead, _edgeDetail, _panelDetail, Dimension.Zero, _paintColor);

    }

}
