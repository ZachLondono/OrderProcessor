using ClosedXML.Excel;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;

public class HafeleMDFDoorOrder {

    public Options Options { get; }
    public Size[] Sizes { get; }
    public Data Data { get; }

    public HafeleMDFDoorOrder(Options options, Size[] sizes, Data data) {
        Options = options;
        Sizes = sizes;
        Data = data;
    }

    public static ParsingResult<HafeleMDFDoorOrder> Load(string workbookPath) {

        using Stream stream = new FileStream(workbookPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var wb = new XLWorkbook(stream);

        List<string> errors = [];
        List<string> warnings = [];

        var result = Options.LoadFromWorkbook(wb);
        Options? options = result.Data;
        errors.AddRange(result.Errors);
        warnings.AddRange(result.Warnings);

        var sizes = Size.LoadFromWorkbook(wb);
        var data = Data.LoadFromWorkbook(wb);

        HafeleMDFDoorOrder? order = null;
        if (options is not null && sizes is not null && data is not null) {
            order = new HafeleMDFDoorOrder(options, sizes, data);
        }

        return new(warnings, errors, order);

    }

}
