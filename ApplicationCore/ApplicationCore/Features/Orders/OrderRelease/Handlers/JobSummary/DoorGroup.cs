namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

internal class DoorGroup {

    public string Room { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public string Finish { get; set; } = string.Empty;

    public List<DoorItem> Items { get; set; } = new();

    public class DoorGroupComparer : IEqualityComparer<DoorGroup> {

        bool IEqualityComparer<DoorGroup>.Equals(DoorGroup? x, DoorGroup? y) {

            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Room == y.Room
                && x.Material == y.Material
                && x.Style == y.Style
                && x.Finish == y.Finish;

        }

        int IEqualityComparer<DoorGroup>.GetHashCode(DoorGroup obj) {
            return HashCode.Combine(obj.Room, obj.Material, obj.Style, obj.Finish);
        }

    }

}
