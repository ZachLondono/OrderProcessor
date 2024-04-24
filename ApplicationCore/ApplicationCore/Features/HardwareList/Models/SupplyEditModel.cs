namespace ApplicationCore.Features.HardwareList.Models;

public class SupplyEditModel(Guid id, int qty, string description) {

    public bool IsDirty { get; set; } = false;

    public Guid Id { get; init; } = id;

    private int _qty = qty;
    public int Qty {
        get => _qty;
        set {
            _qty = value;
            IsDirty = true;
        }
    }

    private string _description = description;
    public string Description {
        get => _description;
        set {
            _description = value;
            IsDirty = true;
        }
    }

}
