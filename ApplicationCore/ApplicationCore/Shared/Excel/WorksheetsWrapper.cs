using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace ApplicationCore.Shared.Excel;

public class WorksheetsWrapper(Sheets worksheets) : IDisposable {

    private bool _disposedValue;

    public WorksheetWrapper this[object index] {
        get => new(worksheets[index]);
    }

    protected virtual void Dispose(bool disposing) {
        if (_disposedValue) {
            return;
        }

        if (worksheets is not null) {
            _ = Marshal.ReleaseComObject(worksheets);
            //worksheets= null;
        }

        _disposedValue = true;
    }

    ~WorksheetsWrapper() {
        Dispose(disposing: false);
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
