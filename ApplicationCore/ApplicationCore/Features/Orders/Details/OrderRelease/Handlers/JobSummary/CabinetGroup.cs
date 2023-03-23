namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;

internal class CabinetGroup {

    public string Room { get; set; } = string.Empty;
    public string BoxFinish { get; set; } = string.Empty;
    public string BoxCore { get; set; } = string.Empty;
    public string FinishFinish { get; set; } = string.Empty;
    public string FinishCore { get; set; } = string.Empty;
    public string Paint { get; set; } = string.Empty;
    public string Fronts { get; set; } = string.Empty;

    public List<CabinetItem> Items { get; set; } = new();

    public class CabinetGroupComparer : IEqualityComparer<CabinetGroup> {

        bool IEqualityComparer<CabinetGroup>.Equals(CabinetGroup? x, CabinetGroup? y) {

            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Room == y.Room
                && x.BoxFinish == y.BoxFinish
                && x.BoxCore == y.BoxCore
                && x.FinishFinish == y.FinishFinish
                && x.FinishCore == y.FinishCore
                && x.Paint == y.Paint
                && x.Fronts == y.Fronts;

        }

        int IEqualityComparer<CabinetGroup>.GetHashCode(CabinetGroup obj) {
            return HashCode.Combine(obj.Room, obj.BoxFinish, obj.BoxCore, obj.FinishFinish, obj.FinishCore, obj.Paint, obj.Fronts);
        }

    }

}
