namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

internal class DrawerBoxGroup {

    public string Room { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string BottomMaterial { get; set; } = string.Empty;
    public string Clips { get; set; } = string.Empty;
    public string Notch { get; set; } = string.Empty;

    public List<DrawerBoxItem> Items { get; set; } = new();

    public class DrawerBoxGroupComparer : IEqualityComparer<DrawerBoxGroup> {

        bool IEqualityComparer<DrawerBoxGroup>.Equals(DrawerBoxGroup? x, DrawerBoxGroup? y) {

            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Room == y.Room
                && x.Material == y.Material
                && x.BottomMaterial == y.BottomMaterial
                && x.Clips == y.Clips
                && x.Notch == y.Notch;

        }

        int IEqualityComparer<DrawerBoxGroup>.GetHashCode(DrawerBoxGroup obj) {
            return HashCode.Combine(obj.Room, obj.Material, obj.BottomMaterial, obj.Clips, obj.Notch);
        }

    }

}
