using Domain.ValueObjects;

namespace ApplicationCore.Features.HardwareList.Models;

public class DrawerSlideEditModel(Guid id, int qty, Dimension length, string style) {

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

    private string _style = style;
    public string Style {
        get => _style;
        set {
            _style = value;
            IsDirty = true;
        }
    }

}
