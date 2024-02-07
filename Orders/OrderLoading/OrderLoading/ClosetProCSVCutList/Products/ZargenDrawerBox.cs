using Domain.Orders.Entities.Products;

namespace OrderLoading.ClosetProCSVCutList.Products;

public class ZargenDrawerBox : IClosetProProduct {

    public string Room => throw new NotImplementedException();

    public int PartNumber => throw new NotImplementedException();

    public IProduct ToProduct() => throw new NotImplementedException();

}
