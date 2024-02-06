using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace Domain.Excel.Wrapper;

public class WorksheetWrapper(Worksheet worksheet) : IDisposable {

    private bool _disposedValue;

    public RangeIndexWrapper Range => new(worksheet);

    public void Select() => worksheet.Select();

    public void ExportAsFixedFormat2(XlFixedFormatType type, string fileName, bool openAfterPublish = false) {
        worksheet.ExportAsFixedFormat2(type, Filename: fileName, OpenAfterPublish: openAfterPublish);
    }

    protected virtual void Dispose(bool disposing) {
        if (_disposedValue) {
            return;
        }

        if (worksheet is not null) _ = Marshal.ReleaseComObject(worksheet);

        _disposedValue = true;
    }

    ~WorksheetWrapper() {
        Dispose(disposing: false);
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
