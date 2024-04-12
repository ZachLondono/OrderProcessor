namespace ApplicationCore.Features.HardwareList.Models;

public class SupplyEditModel {

    public bool IsDirty { get; set; } = false;

    public required Guid Id { get; init; }

    private int _qty;
    public required int Qty {
        get => _qty;
        set {
            _qty = value;
            IsDirty = true;
        }
    }

    private string _description = string.Empty;
    public required string Description {
        get => _description;
        set {
            _description = value;
            IsDirty = true;
        }
    }

}
