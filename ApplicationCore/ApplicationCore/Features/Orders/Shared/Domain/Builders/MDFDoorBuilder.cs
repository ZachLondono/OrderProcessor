using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

public class MDFDoorBuilder {

    private int _qty;
    private string _material;
    private string _framingBead;
    private string _edgeDetail;
    private DoorFrame _frameSize;

    public MDFDoorBuilder(MDFDoorConfiguration configuration) {

        _material = configuration.Material;
        _framingBead = configuration.FramingBead;
        _edgeDetail = configuration.EdgeDetail;
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

    public MDFDoorBuilder WithFramingBead(string framingBead) {
        _framingBead = framingBead;
        return this;
    }

    public MDFDoorBuilder WithEdgeDetail(string edgeDetail) {
        _edgeDetail = edgeDetail;
        return this;
    }

    public MDFDoorBuilder WithFrameSize(DoorFrame frameSize) {
        _frameSize = frameSize;
        return this;
    }

    public MDFDoor Build(Dimension height, Dimension width) {

        return new MDFDoor(_qty, height, width, _material, _framingBead, _edgeDetail, _frameSize, Dimension.Zero);

    }

}
