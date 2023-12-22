using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Components;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;

public class DovetailDrawerBoxProduct : DovetailDrawerBox, IProduct, IDovetailDrawerBoxContainer {

    public Guid Id { get; }
    public decimal UnitPrice { get; }
    public string Room { get; set; }
    public List<string> ProductionNotes { get; set; } = new();

    public DovetailDrawerBoxProduct(Guid id, decimal unitPrice, int qty, string room, int productNumber, Dimension height, Dimension width, Dimension depth, string note, IReadOnlyDictionary<string, string> labelFields, DovetailDrawerBoxConfig options)
                            : base(qty, productNumber, height, width, depth, note, options, labelFields) {
        Id = id;
        UnitPrice = unitPrice;
        Room = room;
    }

    public static DovetailDrawerBoxProduct Create(decimal unitPrice, int qty, string room, int productNumber, Dimension height, Dimension width, Dimension depth, string note, IReadOnlyDictionary<string, string> labelFields, DovetailDrawerBoxConfig options) {
        return new(Guid.NewGuid(), unitPrice, qty, room, productNumber, height, width, depth, note, labelFields, options);
    }

    public bool ContainsDovetailDrawerBoxes() => true;

    public IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder) { yield return this; }

    public IEnumerable<Supply> GetSupplies() => Enumerable.Empty<Supply>();

}
