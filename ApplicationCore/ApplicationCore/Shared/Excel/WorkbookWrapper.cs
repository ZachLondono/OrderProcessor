using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace ApplicationCore.Shared.Excel;

public class WorkbookWrapper(Workbook workbook) : IDisposable {

    private bool _disposedValue;
    private WorksheetsWrapper? _worksheets;

    public WorksheetsWrapper Worksheets {
        get {
            _worksheets ??= new(workbook.Worksheets);
            return _worksheets;
        }
    }

    public WorksheetWrapper ActiveSheet => new(workbook.ActiveSheet);

    protected virtual void Dispose(bool disposing) {
        if (_disposedValue) {
            return;
        }

        if (workbook is not null) {
            workbook.Close(SaveChanges: false);
            _ = Marshal.ReleaseComObject(workbook);
            //workbook = null;
        }

        _disposedValue = true;
    }

    ~WorkbookWrapper() {
        Dispose(disposing: false);
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
