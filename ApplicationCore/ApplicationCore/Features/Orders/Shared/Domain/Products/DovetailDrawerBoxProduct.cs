using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public class DovetailDrawerBoxProduct : DovetailDrawerBox, IProduct, IDrawerBoxContainer {

    public Guid Id { get; }
    public decimal UnitPrice { get; }

    public string Description {
        get {

            string description = "Dovetail Drawer Box";

            if (Options.UBoxDimensions is not null) {
                description = "U-Shaped " + description;
            }

            if (Options.Logo != Enums.LogoPosition.None) {
                description += ", Logo";
            }

            if (Options.ScoopFront) {
                description += ", Scoop Front";
            }

            if (!string.IsNullOrWhiteSpace(Options.Accessory)) {
                description += $", {Options.Accessory}";
            }

            if (Options.FixedDivdersCounts is not null) {
                description += ", Fixed Dividers";
            }

            if (Options.PostFinish) {
                description += ", Finished";
            }

            return description;

        }
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

}
