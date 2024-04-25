using Domain.Orders.Entities.Products;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Entities.Products.DrawerBoxes;

namespace ApplicationCore.Features.Orders.Details.Models;

public class RoomModel {

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
    public List<CabinetPart> CabinetParts { get; private set; }
    public List<IClosetPartProduct> ClosetParts { get; private set; }
    public List<ZargenDrawer> ZargenDrawers { get; private set; }
    public List<DovetailDrawerBoxProduct> DovetailDrawerBoxes { get; private set; }
    public List<DoweledDrawerBoxProduct> DoweledDrawerBoxes { get; private set; }
    public List<MDFDoorProduct> MDFDoors { get; private set; }
    public List<FivePieceDoorProduct> FivePieceDoors { get; private set; }
    public List<CounterTop> CounterTops { get; private set; }

    public RoomModel(string name, List<IProduct> products) {

        _name = name;
        Products = products;

        Cabinets = products.OfType<Cabinet>()
                          .ToList();

        CabinetParts = products.OfType<CabinetPart>()
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

        FivePieceDoors = products.OfType<FivePieceDoorProduct>()
                            .ToList();

        CounterTops = products.OfType<CounterTop>()
                            .ToList();

    }

    public static RoomModel FromGrouping(IGrouping<string, IProduct> productGrouping) {
        return new(productGrouping.Key, productGrouping.ToList());
    }

}
