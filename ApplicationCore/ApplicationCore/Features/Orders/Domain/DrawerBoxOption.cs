namespace ApplicationCore.Features.Orders.Domain;

public class DrawerBoxOption {

    public Guid Id { get;}

    public string Name { get; } = string.Empty;

    public DrawerBoxOption(Guid id, string name) { 
        Id = id;
        Name = name;
    }

}