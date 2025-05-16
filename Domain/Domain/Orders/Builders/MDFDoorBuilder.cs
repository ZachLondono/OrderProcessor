using Domain.Orders.Components;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using OneOf.Types;

namespace Domain.Orders.Builders;

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
    private DoorOrientation _orientation;
    private bool _isOpenPanel;
    private AdditionalOpening[] _additionalOpenings;
    private MDFDoorFinish _finish;

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
        _orientation = _orientation = DoorOrientation.Vertical;
        _additionalOpenings = Array.Empty<AdditionalOpening>();
        _finish = new None();
        _isOpenPanel = false;

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

    public MDFDoorBuilder WithOrientation(DoorOrientation orientation) {
        _orientation = orientation;
        return this;
    }

    public MDFDoorBuilder WithAdditionalOpenings(AdditionalOpening[] additionalOpenings) {
        _additionalOpenings = additionalOpenings;
        return this;
    }

    public MDFDoorBuilder WithIsOpenPanel(bool isOpenPanel) {
        _isOpenPanel = isOpenPanel;
        return this;
    }

    public MDFDoorBuilder WithFinish(MDFDoorFinish finish) {
        _finish = finish;
        return this;
    }

    public MDFDoor Build(Dimension height, Dimension width) {

        return new MDFDoor(_qty, _productNumber, _type, height, width, _note, _frameSize, _material, _thickness, _framingBead, _edgeDetail, _panelDetail, Dimension.Zero, _orientation, _additionalOpenings, _finish, _isOpenPanel);

    }

}
