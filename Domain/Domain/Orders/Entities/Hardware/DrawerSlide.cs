using Domain.ValueObjects;

namespace Domain.Orders.Entities.Hardware;

public record DrawerSlide(Guid Id, int Qty, Dimension Length, string Style) {

    public static DrawerSlide UndermountSlide(int qty, Dimension length) => new(Guid.NewGuid(), qty, length, "Hettich Undermount Slide");

    public static DrawerSlide SidemountSlide(int qty, Dimension length) => new(Guid.NewGuid(), qty, length, "Sidemount Slide");

};
