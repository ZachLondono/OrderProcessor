using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders;

public class DrawerBoxBuilder {

    private Guid _id = Guid.NewGuid();
    private decimal _unitPrice = decimal.Zero;
    private int _qty = 0;
    private string _room = string.Empty;
    private int _productNumber = 0;
    private Dimension _height = Dimension.FromMillimeters(0);
    private Dimension _width = Dimension.FromMillimeters(0);
    private Dimension _depth = Dimension.FromMillimeters(0);
    private string _note = string.Empty;
    private Dictionary<string, string> _labelFields = new();
    private DrawerBoxOptions _options = new(
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        LogoPosition.None);

    public DrawerBoxBuilder WithId(Guid id) {
        _id = id;
        return this;
    }

    public DrawerBoxBuilder WithUnitPrice(decimal unitPrice) {
        _unitPrice = unitPrice;
        return this;
    }

    public DrawerBoxBuilder WithQty(int qty) {
        _qty = qty;
        return this;
    }

    public DrawerBoxBuilder WithRoom(string room) {
        _room = room;
        return this;
    }

    public DrawerBoxBuilder WithHeight(Dimension height) {
        _height = height;
        return this;
    }

    public DrawerBoxBuilder WithWidth(Dimension width) {
        _width = width;
        return this;
    }

    public DrawerBoxBuilder WithDepth(Dimension depth) {
        _depth = depth;
        return this;
    }

    public DrawerBoxBuilder WithOptions(DrawerBoxOptions options) {
        _options = options;
        return this;
    }

    public DrawerBoxBuilder WithNote(string note) {
        _note = note;
        return this;
    }

    public DrawerBoxBuilder WithLabelFields(Dictionary<string, string> fields) {
        _labelFields = fields;
        return this;
    }

    public DrawerBoxBuilder WithProductNumber(int productNumber) {
        _productNumber = productNumber;
        return this;
    }

    public DovetailDrawerBoxProduct Build() => new DovetailDrawerBoxProduct(_id, _unitPrice, _qty, _room, _productNumber, _height, _width, _depth, _note, _labelFields, _options);

}