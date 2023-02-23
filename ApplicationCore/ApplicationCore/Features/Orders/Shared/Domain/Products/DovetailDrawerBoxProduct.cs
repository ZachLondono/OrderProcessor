using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public class DovetailDrawerBoxProduct : DovetailDrawerBox, IProduct, IDrawerBoxContainer {

    public Guid Id { get; }
    public decimal UnitPrice { get; }

    public string GetDescription() {

        string description = "Dovetail Drawer Box";

        if (DrawerBoxOptions.UBoxDimensions is not null) {
            description = "U-Shaped " + description;
        }

        if (DrawerBoxOptions.Logo != Enums.LogoPosition.None) {
            description += ", Logo";
        }

        if (DrawerBoxOptions.ScoopFront) {
            description += ", Scoop Front";
        }

        if (!string.IsNullOrWhiteSpace(DrawerBoxOptions.Accessory)) {
            description += $", {DrawerBoxOptions.Accessory}";
        }

        if (DrawerBoxOptions.FixedDivdersCounts is not null) {
            description += ", Fixed Dividers";
        }

        if (DrawerBoxOptions.PostFinish) {
            description += ", Finished";
        }

        return description;

    }

    public DovetailDrawerBoxProduct(Guid id, decimal unitPrice, int qty, int productNumber, Dimension height, Dimension width, Dimension depth, string note, IReadOnlyDictionary<string, string> labelFields, DrawerBoxOptions options)
                            : base(qty, productNumber, height, width, depth, note, options, labelFields) {
        Id = id;
        UnitPrice = unitPrice;
    }

    public static DovetailDrawerBoxProduct Create(decimal unitPrice, int qty, int productNumber, Dimension height, Dimension width, Dimension depth, string note, IReadOnlyDictionary<string, string> labelFields, DrawerBoxOptions options) {
        return new(Guid.NewGuid(), unitPrice, qty, productNumber, height, width, depth, note, labelFields, options);
    }

    public IEnumerable<DovetailDrawerBox> GetDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) { yield return this; }

    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>();

}
