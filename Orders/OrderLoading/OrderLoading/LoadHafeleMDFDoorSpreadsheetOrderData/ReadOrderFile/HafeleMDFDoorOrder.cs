using ClosedXML.Excel;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;

public class HafeleMDFDoorOrder {

    public Options Options { get; }
    public Size[] Sizes { get; }

    public HafeleMDFDoorOrder(Options options, Size[] sizes) {
        Options = options;
        Sizes = sizes;
    }

    public static ParsingResult<HafeleMDFDoorOrder> Load(string workbookPath) {

        var wb = new XLWorkbook(workbookPath);

        List<string> errors = [];
        List<string> warnings = [];

        var result = Options.LoadFromWorkbook(wb);
        Options? options = result.Data;
        errors.AddRange(result.Errors);
        warnings.AddRange(result.Warnings);

        var sizes = Size.LoadFromWorkbook(wb);

        HafeleMDFDoorOrder? data = null;
        if (options is not null && sizes is not null) {
            data = new HafeleMDFDoorOrder(options, sizes);
        }


        return new(warnings, errors, data);

    }

}
