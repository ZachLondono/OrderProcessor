namespace OrderExporting.JobSummary;

public class DovetailDrawerBoxGroup {

    public string Room { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string BottomMaterial { get; set; } = string.Empty;
    public string Clips { get; set; } = string.Empty;
    public string Notch { get; set; } = string.Empty;

    public List<DovetailDrawerBoxItem> Items { get; set; } = new();

    public class DrawerBoxGroupComparer : IEqualityComparer<DovetailDrawerBoxGroup> {

        bool IEqualityComparer<DovetailDrawerBoxGroup>.Equals(DovetailDrawerBoxGroup? x, DovetailDrawerBoxGroup? y) {

            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Room == y.Room
                && x.Material == y.Material
                && x.BottomMaterial == y.BottomMaterial
                && x.Clips == y.Clips
                && x.Notch == y.Notch;

        }

        int IEqualityComparer<DovetailDrawerBoxGroup>.GetHashCode(DovetailDrawerBoxGroup obj) {
            return HashCode.Combine(obj.Room, obj.Material, obj.BottomMaterial, obj.Clips, obj.Notch);
        }

    }

}
