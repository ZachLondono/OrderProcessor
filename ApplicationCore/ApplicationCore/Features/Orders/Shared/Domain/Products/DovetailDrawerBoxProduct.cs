using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public class DovetailDrawerBoxProduct : DovetailDrawerBox, IProduct, IDrawerBoxContainer {

    public Guid Id { get; }
    public decimal UnitPrice { get; }

    public DovetailDrawerBoxProduct(Guid id, decimal unitPrice, int qty, Dimension height, Dimension width, Dimension depth, string note, IReadOnlyDictionary<string, string> labelFields, DrawerBoxOptions options)
                            : base(qty, height, width, depth, note, options, labelFields) {
        Id = id;
        UnitPrice = unitPrice;
    }

    public static DovetailDrawerBoxProduct Create(decimal unitPrice, int qty, Dimension height, Dimension width, Dimension depth, string note, IReadOnlyDictionary<string, string> labelFields, DrawerBoxOptions options) {
        return new(Guid.NewGuid(), unitPrice, qty, height, width, depth, note, labelFields, options);
    }

    public IEnumerable<DovetailDrawerBox> GetDrawerBoxes() { yield return this; }

}
