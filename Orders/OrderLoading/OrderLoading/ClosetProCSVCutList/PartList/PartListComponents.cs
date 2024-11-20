using Domain.Orders.Entities.Hardware;
using Domain.Orders.Entities.Products;

namespace OrderLoading.ClosetProCSVCutList.PartList;

public record PartListComponents(
                IProduct[] Products,
                Supply[] Supplies,
                HangingRail[] HangingRails,
                DrawerSlide[] DrawerSlides);