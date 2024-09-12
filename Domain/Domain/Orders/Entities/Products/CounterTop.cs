using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Products;

public class CounterTop(Guid id,
                  int qty,
                  decimal unitPrice,
                  int productNumber,
                  string room,
                  List<ProductionNote> productionNotes,
                  string finish,
                  Dimension width,
                  Dimension length,
                  EdgeBandingSides edgeBanding) : IProduct {

    public Guid Id { get; } = id;
    public int Qty { get; } = qty;
    public decimal UnitPrice { get; } = unitPrice;
    public int ProductNumber { get; } = productNumber;
    public string Room { get; set; } = room;
    public List<ProductionNote> ProductionNotes { get; } = productionNotes;
    public string Finish { get; } = finish;
    public Dimension Width { get; } = width;
    public Dimension Length { get; } = length;
    public EdgeBandingSides EdgeBanding { get; } = edgeBanding;

    public string GetDescription() => $"'{Finish}' Counter Top";

}
