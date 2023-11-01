namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.FivePieceDoorCutList;

public interface IFivePieceDoorCutListWriter {
    Action<string>? OnError { get; set; }

    CutListResult? WriteCutList(FivePieceCutList cutList, string outputDirectory, bool generatePDF);
}