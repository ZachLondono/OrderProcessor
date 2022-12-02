using ApplicationCore.Features.Orders.Domain;

namespace ApplicationCore.Tests.Unit.Orders;

internal class OrderBuilder {

    private Guid _id = Guid.NewGuid();
    private string _source = string.Empty;
    private Status _status = Status.UNKNOWN;
    private string _number = string.Empty;
    private string _name = string.Empty;
    private Guid _customerId = Guid.NewGuid();
    private Guid _vendorId = Guid.NewGuid();
    private string _note = string.Empty;
    private string _comment = string.Empty;
    private DateTime _orderDate = DateTime.Today;
    private DateTime? _releaseDate = null;
    private DateTime? _completeDate = null;
    private DateTime? _productionDate = null;
    private decimal _tax = decimal.Zero;
    private decimal _shipping = decimal.Zero;
    private decimal _priceAdjustment = decimal.Zero;
    private bool _rush = false;
    private Dictionary<string,string> _info = new();
    private List<AdditionalItem> _items = new();
    private List<DrawerBox> _boxes = new();

    public OrderBuilder WithId(Guid id) {
        _id = id;
        return this;
    }
    
    public OrderBuilder WithSource(string source) {
        _source = source;
        return this;
    }
    
    public OrderBuilder WithStatus(Status status) {
        _status = status;
        return this;
    }

    public OrderBuilder WithNumber(string number) {
        _number = number;
        return this;
    }

    public OrderBuilder WithName(string name) {
        _name = name;
        return this;
    }

    public OrderBuilder WithCustomerId(Guid customerId) {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder WithVendorId(Guid vendorId) {
        _vendorId = vendorId;
        return this;
    }

    public OrderBuilder WithNote(string note) {
        _note = note;
        return this;
    }

    public OrderBuilder WithComment(string comment) {
        _comment = comment;
        return this;
    }

    public OrderBuilder WithOrderDate(DateTime orderDate) {
        _orderDate = orderDate;
        return this;
    }

    public OrderBuilder WithReleaseDate(DateTime? releaseDate) {
        _releaseDate = releaseDate;
        return this;
    }

    public OrderBuilder WithProductionDate(DateTime? productionDate) {
        _productionDate = productionDate;
        return this;
    }

    public OrderBuilder WithCompleteDate(DateTime? completeDate) {
        _completeDate = completeDate;
        return this;
    }

    public OrderBuilder WithTax(decimal tax) {
        _tax = tax;
        return this;
    }

    public OrderBuilder WithShipping(decimal shipping) {
        _shipping = shipping;
        return this;
    }

    public OrderBuilder WithPriceAdjustment(decimal priceAdjustment) {
        _priceAdjustment = priceAdjustment;
        return this;
    }

    public OrderBuilder WithInfo(Dictionary<string, string> info) {
        _info = info;
        return this;
    }

    public OrderBuilder WithBoxes(List<DrawerBox> boxes) {
        _boxes = boxes;
        return this;
    }

    public OrderBuilder WithItems(List<AdditionalItem> items) {
        _items = items;
        return this;
    }

    public OrderBuilder WithRush(bool rush) {
        _rush = rush;
        return this;
    }

    public Order Buid() => new(_id, _source, _status, _number, _name, _customerId, _vendorId, _note, _comment, _orderDate, _releaseDate, _productionDate, _completeDate, _tax, _shipping, _priceAdjustment, _rush, _info, _boxes, _items);

}
