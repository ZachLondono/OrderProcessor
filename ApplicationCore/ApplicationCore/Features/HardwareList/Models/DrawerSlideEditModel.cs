using Domain.ValueObjects;

namespace ApplicationCore.Features.HardwareList.Models;

public class DrawerSlideEditModel {

    public bool IsDirty { get; set; }

    public required Guid Id { get; init; }

    private int _qty;
    public required int Qty {
        get => _qty;
        set {
            _qty = value;
            IsDirty = true;
        }
    }

    private Dimension _length;
    public required Dimension Length {
        get => _length;
        set {
            _length = value;
            IsDirty = true;
        }
    }

    private string _style = string.Empty;
    public required string Style {
        get => _style;
        set {
            _style = value;
            IsDirty = true;
        }
    }

}
