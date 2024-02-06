using Microsoft.Office.Interop.Excel;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace Domain.Excel.Wrapper;

public class RangeIndexWrapper(Worksheet workbook) {

    public Range this[object cell1, object? cell2 = null] {
        get {
            if (cell2 != null) {
                return workbook.Range[cell1, cell2];
            }
            return workbook.Range[cell1];
        }
    }

}
