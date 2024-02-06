using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace Domain.Excel.Wrapper;

public class WorkbooksWrapper(Workbooks workbooks) : IDisposable {

    private bool _disposedValue;

    public WorkbookWrapper Open(string fileName, bool readOnly = false) {
        return new(workbooks.Open(fileName, ReadOnly: readOnly));
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposedValue) {

            if (workbooks is not null) {
                workbooks.Close();
                _ = Marshal.ReleaseComObject(workbooks);
                //workbooks = null;
            }

            _disposedValue = true;

        }
    }

    ~WorkbooksWrapper() {
        Dispose(disposing: false);
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
