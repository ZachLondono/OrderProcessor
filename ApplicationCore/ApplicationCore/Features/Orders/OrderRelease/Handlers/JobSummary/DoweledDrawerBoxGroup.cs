using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

internal class DoweledDrawerBoxGroup {

    public string Room { get; set; } = string.Empty;
    public DoweledDrawerBoxMaterial FrontMaterial { get; set; } = new("", Dimension.Zero, false);
    public DoweledDrawerBoxMaterial BackMaterial { get; set; } = new("", Dimension.Zero, false);      
    public DoweledDrawerBoxMaterial SideMaterial { get; set; } = new("", Dimension.Zero, false);
    public DoweledDrawerBoxMaterial BottomMaterial { get; set; } = new("", Dimension.Zero, false);
    public bool MachineForUMSlides { get; set; }

    public List<DoweledDrawerBoxItem> Items { get; set; } = new();

    public class DoweledDrawerBoxGroupComparer : IEqualityComparer<DoweledDrawerBoxGroup> {

        bool IEqualityComparer<DoweledDrawerBoxGroup>.Equals(DoweledDrawerBoxGroup? x, DoweledDrawerBoxGroup? y) {
 
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Room == y.Room
                && x.FrontMaterial == y.FrontMaterial
                && x.BackMaterial == y.BackMaterial
                && x.SideMaterial == y.SideMaterial
                && x.BottomMaterial == y.BottomMaterial
                && x.MachineForUMSlides == y.MachineForUMSlides;

        }

        int IEqualityComparer<DoweledDrawerBoxGroup>.GetHashCode(DoweledDrawerBoxGroup obj) {
            return HashCode.Combine(obj.FrontMaterial, obj.BackMaterial, obj.SideMaterial, obj.BottomMaterial, obj.MachineForUMSlides);
        }

    }

}
