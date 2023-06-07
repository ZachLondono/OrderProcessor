using ApplicationCore.Features.Orders.Shared.Domain.Products;
using static ApplicationCore.Features.Orders.Details.ProductTables.CabinetProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.ClosetPartProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.DovetailDrawerBoxProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.MDFDoorProductTable;

namespace ApplicationCore.Features.Orders.Details;

public class Room {

    public required string Name { get; set; }
    public required List<CabinetRowModel> Cabinets { get; init; } 
    public required List<ClosetPartRowModel> ClosetParts { get; init; }
    public required List<DovetailDrawerBoxRowModel> DovetailDrawerBoxes { get; init; }
    public required List<MDFDoorRowModel> MDFDoors { get; init; }

    public static Room FromGrouping(IGrouping<string, IProduct> productGrouping) {

        var cabinets = productGrouping
                            .OfType<Cabinet>()
                            .Select(cab => new CabinetRowModel(cab))
                            .ToList();

        var closetParts = productGrouping
                                .OfType<ClosetPart>()
                                .Select(cp => new ClosetPartRowModel(cp))
                                .ToList();

        var drawerBoxes = productGrouping
                                .OfType<DovetailDrawerBoxProduct>()
                                .Select(db => new DovetailDrawerBoxRowModel(db))
                                .ToList();

        var doors = productGrouping 
                        .OfType<MDFDoorProduct>()
                        .Select(door => new MDFDoorRowModel(door))
                        .ToList();

        return new() {
            Name = productGrouping.Key,
            Cabinets = cabinets,
            ClosetParts = closetParts,
            DovetailDrawerBoxes = drawerBoxes,
            MDFDoors = doors
        };

    }

}
