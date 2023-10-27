using ApplicationCore.Shared.Services;
using Microsoft.Win32;
using System;
using System.IO;

namespace DesktopHost.Dialogs;

public class FilePicker : IFilePicker {

    public void PickFile(FilePickerOptions options, Action<string> onFilePicked) {
        PickFile(options, false, (dialog) => {
            onFilePicked(dialog.FileName);
        }, () => { });
    }

    public void PickFile(FilePickerOptions options, Action<string> onFilePicked, Action onCanceled) {
        PickFile(options, false, (dialog) => {
            onFilePicked(dialog.FileName);
        }, onCanceled);
    }

    public void PickFiles(FilePickerOptions options, Action<string[]> onFilePicked) {
        PickFile(options, true, (dialog) => {
            onFilePicked(dialog.FileNames);
        }, () => { });
    }

    private static void PickFile(FilePickerOptions options, bool multiSelect, Action<OpenFileDialog> onFilePicked, Action onCanceled) {

        /*
         * Show a modal dialog after the current event handler is completed, to avoid potential reentrancy caused by running a nested message loop in the WebView2 event handler.
         * For more information on WebView2 reentrancy:
         * https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/threading-model#re-entrancy
         * https://github.com/MicrosoftEdge/WebView2Feedback/issues/2542
         */

        System.Threading
                .SynchronizationContext
                .Current?
                .Post((_) => {

                    if (!string.IsNullOrWhiteSpace(options.InitialDirectory) && !Directory.Exists(options.InitialDirectory)) {
                        options.InitialDirectory = string.Empty;
                    }

                    var dialog = new OpenFileDialog {
                        InitialDirectory = options.InitialDirectory,
                        Multiselect = multiSelect,
                        Title = options.Title,
                        Filter = options.Filter.ToFilterString(),
                    };

                    bool? result = dialog.ShowDialog();

                    if (result is not null && (bool)result) {
                        onFilePicked(dialog);
                    } else {
                        onCanceled();
                    }

                }, null);

    }

}
