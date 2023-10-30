namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

internal class FivePieceDoorGroup {

    public string Room { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;

    public List<FivePieceDoorItem> Items { get; set; } = new();

    public class FivePieceDoorGroupComparer : IEqualityComparer<FivePieceDoorGroup> {

        bool IEqualityComparer<FivePieceDoorGroup>.Equals(FivePieceDoorGroup? x, FivePieceDoorGroup? y) {

            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Room == y.Room
                && x.Material == y.Material;

        }

        int IEqualityComparer<FivePieceDoorGroup>.GetHashCode(FivePieceDoorGroup obj) {
            return HashCode.Combine(obj.Room, obj.Material);
        }

    }

}
