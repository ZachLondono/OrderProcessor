using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Tests.Unit.Orders;

public class AdditionalItemBuilder {

    private Guid _id = Guid.NewGuid();
    private string _description = string.Empty;
    private decimal _price = decimal.Zero;

    public AdditionalItemBuilder WithId(Guid id) {
        _id = id;
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

    public AdditionalItem Build() => new(_id, _description, _price);

}