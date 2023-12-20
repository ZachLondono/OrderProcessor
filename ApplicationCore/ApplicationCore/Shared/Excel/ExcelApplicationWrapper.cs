using System.Runtime.InteropServices;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;

namespace ApplicationCore.Shared.Excel;

public class ExcelApplicationWrapper : IDisposable {

    private ExcelApp? _app;
    private bool _disposedValue;
    private WorkbooksWrapper? _workbooks = null;

    public bool Visible {
        get => _app!.Visible;
        set => _app!.Visible = value;
    }

    public bool DisplayAlerts {
        get => _app!.DisplayAlerts;
        set => _app!.DisplayAlerts = value;
    }

    public WorkbooksWrapper Workbooks {
        get {
            _workbooks ??= new WorkbooksWrapper(_app!.Workbooks);
            return _workbooks;
        }
    }

    public ExcelApplicationWrapper() {
        _app = new();

        if (_app is null) throw new InvalidOperationException("Failed to create instance of Excel Application");
    }

    public object? RunMacro(string workbookName, string macroName) {

        if (_app is null) throw new InvalidOperationException("Excel Application not Initialized");

        return _app.GetType()
                .InvokeMember("Run",
                              System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod,
                              null,
                              _app,
                              new object[] { $"'{workbookName}'!{macroName}" });

    }

    protected virtual void Dispose(bool disposing) {

        if (_disposedValue) {
            return;
        }

        if (disposing) {
            _workbooks?.Dispose();
            _workbooks = null;
        }

        if (_app is not null) {

            _app.Quit();
            _ = Marshal.ReleaseComObject(_app);
            _app = null;

            // Clean up COM objects, calling these twice ensures it is fully cleaned up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        _disposedValue = true;

    }

    ~ExcelApplicationWrapper() {
        Dispose(disposing: false);
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
