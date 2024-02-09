namespace OrderExporting.FivePieceDoorCutList;

public interface IFivePieceDoorCutListWriter {
    Action<string>? OnError { get; set; }

    FivePieceDoorCutListResult? WriteCutList(FivePieceCutList cutList, string outputDirectory, bool generatePDF, string? name = "");
}