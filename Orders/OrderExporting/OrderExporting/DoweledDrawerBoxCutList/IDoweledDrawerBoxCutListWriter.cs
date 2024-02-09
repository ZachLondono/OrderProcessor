namespace OrderExporting.DoweledDrawerBoxCutList;

public interface IDoweledDrawerBoxCutListWriter {
    Action<string>? OnError { get; set; }

    public DoweledDBCutListResult? WriteCutList(DoweledDrawerBoxCutList cutList, string outputDirectory, bool generatePDF);
}
