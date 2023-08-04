namespace ApplicationCore.Features.Orders.Shared.Domain.Components;

public interface IComponent {

    public Guid Id { get; set; }
    public int Qty { get; set; }

}
