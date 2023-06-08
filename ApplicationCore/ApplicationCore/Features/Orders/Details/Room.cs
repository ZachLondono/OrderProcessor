using ApplicationCore.Features.Orders.Shared.Domain.Products;
using static ApplicationCore.Features.Orders.Details.ProductTables.CabinetProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.ClosetPartProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.DovetailDrawerBoxProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.MDFDoorProductTable;

namespace ApplicationCore.Features.Orders.Details;

public class Room {

    private string _name;

    public string Name {
        get => _name;
        set {
            _name = value;
            IsDirty = true;
        }
    }
    public List<IProduct> Products { get; init; }
    public bool IsDirty { get; set; }

    public List<CabinetRowModel> Cabinets { get; private set; }
    public List<ClosetPartRowModel> ClosetParts { get; private set; }
    public List<DovetailDrawerBoxRowModel> DovetailDrawerBoxes { get; private set; }
    public List<MDFDoorRowModel> MDFDoors { get; private set; }

    public Room(string name, List<IProduct> products) {

        _name = name;
        Products = products;

        Cabinets = products.OfType<Cabinet>()
                          .Select(cab => new CabinetRowModel(cab))
                          .ToList();

        ClosetParts = products.OfType<ClosetPart>()
                              .Select(cp => new ClosetPartRowModel(cp))
                              .ToList();

        DovetailDrawerBoxes = products.OfType<DovetailDrawerBoxProduct>()
                                  .Select(db => new DovetailDrawerBoxRowModel(db))
                                  .ToList();

        MDFDoors = products.OfType<MDFDoorProduct>()
                        .Select(door => new MDFDoorRowModel(door))
                        .ToList();


    }

    public static Room FromGrouping(IGrouping<string, IProduct> productGrouping) {
        return new(productGrouping.Key, productGrouping.ToList()); 
    }

}
