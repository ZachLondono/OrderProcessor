using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Details.Models.HardwareList;

public class HangingRailEditModel(Guid id, int qty, Dimension length, string finish) {

    public bool IsDirty { get; set; }

    public Guid Id { get; init; } = id;

    private int _qty = qty;
    public int Qty {
        get => _qty;
        set {
            _qty = value;
            IsDirty = true;
        }
    }

    private Dimension _length = length;
    public Dimension Length {
        get => _length;
        set {
            _length = value;
            IsDirty = true;
        }
    }

    private string _finish = finish;
    public string Finish {
        get => _finish;
        set {
            _finish = value;
            IsDirty = true;
        }
    }

}
