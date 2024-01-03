using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Tests.Unit.Orders;

public class AdditionalItemBuilder {

    private Guid _id = Guid.NewGuid();
    private int _quantity = 0;
    private string _description = string.Empty;
    private decimal _price = decimal.Zero;

    public AdditionalItemBuilder WithId(Guid id) {
        _id = id;
        return this;
    }

    public AdditionalItemBuilder WithQuantity(int quantity) {
        _quantity = quantity;
        return this;
    }

    public AdditionalItemBuilder WithDescription(string description) {
        _description = description;
        return this;
    }

    public AdditionalItemBuilder WithPrice(decimal price) {
        _price = price;
        return this;
    }

    public AdditionalItem Build() => new(_id, _quantity, _description, _price);

}