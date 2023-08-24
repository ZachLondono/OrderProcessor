namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

internal class MDFDoorGroup {

    public string Room { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public string Finish { get; set; } = string.Empty;

    public List<MDFDoorItem> Items { get; set; } = new();

    public class DoorGroupComparer : IEqualityComparer<MDFDoorGroup> {

        bool IEqualityComparer<MDFDoorGroup>.Equals(MDFDoorGroup? x, MDFDoorGroup? y) {

            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Room == y.Room
                && x.Material == y.Material
                && x.Style == y.Style
                && x.Finish == y.Finish;

        }

        int IEqualityComparer<MDFDoorGroup>.GetHashCode(MDFDoorGroup obj) {
            return HashCode.Combine(obj.Room, obj.Material, obj.Style, obj.Finish);
        }

    }

}
