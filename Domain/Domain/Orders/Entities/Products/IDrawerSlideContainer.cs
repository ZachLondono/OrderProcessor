using Domain.Orders.Entities.Hardware;

namespace Domain.Orders.Entities.Products;

public interface IDrawerSlideContainer {

    public IEnumerable<DrawerSlide> GetDrawerSlides();

}
