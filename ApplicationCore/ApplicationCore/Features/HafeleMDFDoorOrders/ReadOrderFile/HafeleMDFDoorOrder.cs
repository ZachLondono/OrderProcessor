using ClosedXML.Excel;

namespace ApplicationCore.Features.HafeleMDFDoorOrders.ReadOrderFile;

public class HafeleMDFDoorOrder {

    public Options Options { get; }
    public Size[] Sizes { get; }

    public HafeleMDFDoorOrder(Options options, Size[] sizes) {
        Options = options;
        Sizes = sizes;
    }

    public static HafeleMDFDoorOrder Load(string workbookPath) {

        var wb = new XLWorkbook(workbookPath);

        var options = Options.LoadFromWorkbook(wb);
        var sizes = Size.LoadFromWorkbook(wb);

        return new(options, sizes);

    }

}
