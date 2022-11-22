using ApplicationCore.Features.Orders.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Domain;

public class DrawerBoxMaterial {

    public Guid Id { get; set; }

    public string Name { get; set; }

    public Dimension Thickness { get; set; }

    public DrawerBoxMaterial() {
        Id = Guid.Empty;
        Name = string.Empty;
        Thickness = Dimension.FromMillimeters(0);
    }

    public DrawerBoxMaterial(Guid id, string name, Dimension thickness) {
        Id = id;
        Name = name;
        Thickness = thickness;
    }

}