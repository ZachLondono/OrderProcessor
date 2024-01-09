using ApplicationCore.Features.ClosetProCSVCutList.EqualityComparers;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Shared;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public class ClosetProGroupAccumulator {

    public List<DrawerBox> DrawerBoxes { get; set; } = [];
    public List<CornerShelf> CornerShelves { get; set; } = [];
    public List<DividerShelf> DividerShelves { get; set; } = [];
    public List<DividerVerticalPanel> DividerVerticalPanels { get; set; } = [];
    public List<FivePieceFront> FivePieceFronts { get; set; } = [];
    public List<HutchVerticalPanel> HutchVerticalPanels { get; set; } = [];
    public List<IslandVerticalPanel> IslandVerticalPanels { get; set; } = [];
    public List<MDFFront> MDFFronts { get; set; } = [];
    public List<MelamineSlabFront> MelamineSlabFronts { get; set; } = [];
    public List<MiscellaneousClosetPart> MiscellaneousClosetParts { get; set; } = [];
    public List<Shelf> Shelves { get; set; } = [];
    public List<TransitionVerticalPanel> TransitionVerticalPanels { get; set; } = [];
    public List<VerticalPanel> VerticalPanels { get; set; } = [];
    public List<ZargenDrawerBox> ZargenDrawerBoxes { get; set; } = [];

    public List<IClosetProProduct> UnspecifiedClosetProProducts { get; set; } = [];

    public void AddProduct(DrawerBox product) => DrawerBoxes.Add(product);
    public void AddProduct(CornerShelf product) => CornerShelves.Add(product);
    public void AddProduct(DividerShelf product) => DividerShelves.Add(product);
    public void AddProduct(DividerVerticalPanel product) => DividerVerticalPanels.Add(product);
    public void AddProduct(FivePieceFront product) => FivePieceFronts.Add(product);
    public void AddProduct(HutchVerticalPanel product) => HutchVerticalPanels.Add(product);
    public void AddProduct(IslandVerticalPanel product) => IslandVerticalPanels.Add(product);
    public void AddProduct(MDFFront product) => MDFFronts.Add(product);
    public void AddProduct(MelamineSlabFront product) => MelamineSlabFronts.Add(product);
    public void AddProduct(MiscellaneousClosetPart product) => MiscellaneousClosetParts.Add(product);
    public void AddProduct(Shelf product) => Shelves.Add(product);
    public void AddProduct(TransitionVerticalPanel product) => TransitionVerticalPanels.Add(product);
    public void AddProduct(VerticalPanel product) => VerticalPanels.Add(product);
    public void AddProduct(ZargenDrawerBox product) => ZargenDrawerBoxes.Add(product);
    public void AddProduct(IClosetProProduct product) => UnspecifiedClosetProProducts.Add(product);

    public List<IClosetProProduct> GetGroupedProducts() {

        List<IClosetProProduct> allProducts = [];

        DrawerBoxes.GroupBy(b => b, new DrawerBoxComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        CornerShelves.GroupBy(b => b, new CornerShelfComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        DividerShelves.GroupBy(b => b, new DividerShelfComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        DividerVerticalPanels.GroupBy(b => b, new DividerVerticalPanelComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        FivePieceFronts.GroupBy(b => b, new FivePieceFrontComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        HutchVerticalPanels.GroupBy(b => b, new HutchVerticalPanelComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        IslandVerticalPanels.GroupBy(b => b, new IslandVerticalPanelComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        MDFFronts.GroupBy(b => b, new MDFFrontComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        MelamineSlabFronts.GroupBy(b => b, new MelamineSlabFrontComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        MiscellaneousClosetParts.GroupBy(b => b, new MiscellaneousClosetPartComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        Shelves.GroupBy(b => b, new ShelfComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        TransitionVerticalPanels.GroupBy(b => b, new TransitionVerticalPanelComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        VerticalPanels.GroupBy(b => b, new VerticalPanelComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);

        ZargenDrawerBoxes.ForEach(allProducts.Add);
        /*
        ZargenDrawerBoxes.GroupBy(b => b, new ZargenDrawerBoxComparer())
                    .Select(g => {
                        int totalQty = g.Sum(b => b.Qty);
                        g.Key.Qty = totalQty;
                        return g.Key;
                    })
                    .ForEach(allProducts.Add);
        */

        return allProducts.OrderBy(p =>p.PartNumber).ToList();

    }

}
