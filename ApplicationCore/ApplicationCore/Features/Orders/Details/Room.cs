using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;

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

    public List<Cabinet> Cabinets { get; private set; }
    public List<IClosetPartProduct> ClosetParts { get; private set; }
    public List<ZargenDrawer> ZargenDrawers { get; private set; }
    public List<DovetailDrawerBoxProduct> DovetailDrawerBoxes { get; private set; }
    public List<DoweledDrawerBoxProduct> DoweledDrawerBoxes { get; private set; }
    public List<MDFDoorProduct> MDFDoors { get; private set; }

    public Room(string name, List<IProduct> products) {

        _name = name;
        Products = products;

        Cabinets = products.OfType<Cabinet>()
                          .ToList();

        ClosetParts = products.OfType<IClosetPartProduct>()
                              .ToList();

        ZargenDrawers = products.OfType<ZargenDrawer>()
                              .ToList();

        DovetailDrawerBoxes = products.OfType<DovetailDrawerBoxProduct>()
                                      .ToList();

        DoweledDrawerBoxes = products.OfType<DoweledDrawerBoxProduct>()
                                    .ToList();

        MDFDoors = products.OfType<MDFDoorProduct>()
                            .ToList();

    }

    public static Room FromGrouping(IGrouping<string, IProduct> productGrouping) {
        return new(productGrouping.Key, productGrouping.ToList());
    }

}
