using Microsoft.Office.Interop.Excel;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace ApplicationCore.Shared.Excel;

public class RangeIndexWrapper(Worksheet workbook) {

    public Range this[object cell1, object? cell2 = null] {
        get => workbook.Range[cell1, cell2];
    }

}
